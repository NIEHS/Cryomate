﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace SprayingSystem.IdsDriver
{
    public class BackEnd
    {
        // Event which is raised if a new image was received
        public delegate void ImageReceivedEventHandler(object sender, Bitmap image);
        public event ImageReceivedEventHandler ImageReceived;

        // Event which is raised if the counters has changed
        public delegate void CountersUpdatedEventHandler(object sender, uint frameCounter, uint errorCounter);
        public event CountersUpdatedEventHandler CountersUpdated;

        // Event which is raised if an Error or Exception has occurred
        public delegate void MessageBoxTriggerEventHandler(object sender, String messageTitle, String messageText);
        public event MessageBoxTriggerEventHandler MessageBoxTrigger;

        #region Fields

        private AcquisitionWorker acquisitionWorker;
        private Thread acquisitionThread;

        private peak.core.Device device;
        private peak.core.DataStream dataStream;
        private peak.core.NodeMap nodeMapRemoteDevice;

        private bool isActive;

        #endregion

        #region Public methods

        public BackEnd()
        {
            Debug.WriteLine("--- [BackEnd] Init");
            isActive = true;

            try
            {
                // Create acquisition worker thread that waits for new images from the camera
                acquisitionWorker = new AcquisitionWorker();
                acquisitionThread = new Thread(new ThreadStart(acquisitionWorker.Start));

                acquisitionWorker.ImageReceived += acquisitionWorker_ImageReceived;
                acquisitionWorker.CountersUpdated += acquisitionWorker_CountersUpdated;
                acquisitionWorker.MessageBoxTrigger += acquisitionWorker_MessageBoxTrigger;

                // Initialize peak library
                peak.Library.Initialize();
            }
            catch (Exception e)
            {
                Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
            }
        }

        public bool Start()
        {
            Debug.WriteLine("--- [BackEnd] Start");
            if (!OpenDevice())
            {
                return false;
            }

            // Start thread execution
            acquisitionThread.Start();

            return true;
        }

        public void Stop()
        {
            Debug.WriteLine("--- [BackEnd] Stop");
            isActive = false;
            acquisitionWorker.Stop();

            if (acquisitionThread.IsAlive)
            {
                acquisitionThread.Join();
            }

            CloseDevice();

            // Close peak library
            peak.Library.Close();
        }

        public bool OpenDevice()
        {
            Debug.WriteLine("--- [BackEnd] Open device");
            try
            {
                // Create instance of the device manager
                var deviceManager = peak.DeviceManager.Instance();

                // Update the device manager
                deviceManager.Update();

                // Return if no device was found
                if (!deviceManager.Devices().Any())
                {
                    Debug.WriteLine("--- [BackEnd] Error: No device found");
                    if (MessageBoxTrigger != null)
                        MessageBoxTrigger(this, "Camera Connection Failure", "Could not find the camera.");
                    return false;
                }

                // Open the first openable device in the device manager's device list
                var deviceCount = deviceManager.Devices().Count();

                for (var i = 0; i < deviceCount; ++i)
                {
                    if (deviceManager.Devices()[i].IsOpenable())
                    {
                        device = deviceManager.Devices()[i].OpenDevice(peak.core.DeviceAccessType.Control);

                        // Stop after the first opened device
                        break;
                    }
                    else if (i == (deviceCount - 1))
                    {
                        Debug.WriteLine("--- [BackEnd] Error: Device could not be openend");
                        MessageBoxTrigger(this, "Error", "Device could not be openend");
                        return false;
                    }
                }

                if (device != null)
                {
                    // Check if any datastreams are available
                    var dataStreams = device.DataStreams();

                    if (!dataStreams.Any())
                    {
                        Debug.WriteLine("--- [BackEnd] Error: Device has no DataStream");
                        MessageBoxTrigger(this, "Error", "Device has no DataStream");
                        return false;
                    }

                    // Open standard data stream
                    dataStream = dataStreams[0].OpenDataStream();

                    // Get nodemap of remote device for all accesses to the genicam nodemap tree
                    nodeMapRemoteDevice = device.RemoteDevice().NodeMaps()[0];

                    // To prepare for untriggered continuous image acquisition, load the default user set if available
                    // and wait until execution is finished
                    try
                    {
                        nodeMapRemoteDevice.FindNode<peak.core.nodes.EnumerationNode>("UserSetSelector").SetCurrentEntry("Default");
                        nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("UserSetLoad").Execute();
                        nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("UserSetLoad").WaitUntilDone();
                    }
                    catch
                    {
                        // UserSet is not available
                    }

                    // Get the payload size for correct buffer allocation
                    UInt32 payloadSize = Convert.ToUInt32(nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("PayloadSize").Value());

                    // Get the minimum number of buffers that must be announced
                    var bufferCountMax = dataStream.NumBuffersAnnouncedMinRequired();

                    // Allocate and announce image buffers and queue them
                    for (var bufferCount = 0; bufferCount < bufferCountMax; ++bufferCount)
                    {
                        var buffer = dataStream.AllocAndAnnounceBuffer(payloadSize, IntPtr.Zero);
                        dataStream.QueueBuffer(buffer);
                    }

                    // Configure worker
                    acquisitionWorker.SetDataStream(dataStream);
                    acquisitionWorker.SetNodemapRemoteDevice(nodeMapRemoteDevice);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                if (MessageBoxTrigger != null)
                    MessageBoxTrigger(this, "Exception", e.Message);
                return false;
            }

            return true;
        }

        public void CloseDevice()
        {
            Debug.WriteLine("--- [BackEnd] Close device");
            // If device was opened, try to stop acquisition
            if (device != null)
            {
                try
                {
                    var remoteNodeMap = device.RemoteDevice().NodeMaps()[0];
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").Execute();
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").WaitUntilDone();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                    MessageBoxTrigger(this, "Exception", e.Message);
                }
            }

            // If data stream was opened, try to stop it and revoke its image buffers
            if (dataStream != null)
            {
                try
                {
                    dataStream.KillWait();
                    dataStream.StopAcquisition(peak.core.AcquisitionStopMode.Default);
                    dataStream.Flush(peak.core.DataStreamFlushMode.DiscardAll);

                    foreach (var buffer in dataStream.AnnouncedBuffers())
                    {
                        dataStream.RevokeBuffer(buffer);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                    MessageBoxTrigger(this, "Exception", e.Message);
                }
            }

            try
            {
                // Unlock parameters after acquisition stop
                nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("TLParamsLocked").SetValue(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                MessageBoxTrigger(this, "Exception", e.Message);
            }
        }

        public bool IsActive()
        {
            return isActive;
        }

        #endregion

        #region Private

        private void acquisitionWorker_ImageReceived(object sender, System.Drawing.Bitmap image)
        {
            ImageReceived(sender, image);
        }

        private void acquisitionWorker_CountersUpdated(object sender, uint frameCounter, uint errorCounter)
        {
            if (CountersUpdated != null)
                CountersUpdated(sender, frameCounter, errorCounter);
        }

        private void acquisitionWorker_MessageBoxTrigger(object sender, String messageTitle, String messageText)
        {
            if (MessageBoxTrigger != null)
                MessageBoxTrigger(sender, messageTitle, messageText);
        }

        #endregion
    }

}
