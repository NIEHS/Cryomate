using System;
using SprayingSystem.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using SprayingSystem.ViewModels;

namespace SprayingSystem.SprayingSystemConfigViewer
{
    public class NotifierPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class ParameterNodes : NotifierPropertyChanged
    {
        public string Name { get; set; }

        public ObservableCollection<object> Nodes { get; set; }
    }

    public class Node : NotifierPropertyChanged
    {
        private string _value;
        private bool _isEdited;

        public string Name { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    IsEdited = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                _isEdited = value;
                OnPropertyChanged();
            }
        }

        public Node() { }

        public Node(string name, string value)
        {
            Name = name;
            _value = value;
        }
    }

    public class ModelWrapper : NotifierPropertyChanged
    {
        #region Fields

        private SprayingSystemConfig _data;
        private readonly ObservableCollection<ParameterNodes> _nodes;

        #endregion

        /// <summary>
        /// 'data' can be created calling SprayingSystemConfig.LoadSettings()
        /// </summary>
        public ModelWrapper(SprayingSystemConfig data)
        {
            _data = data;

            _nodes = new ObservableCollection<ParameterNodes>();
            _nodes.Add(CameraInfo());
            _nodes.Add(RpiInfo());
            _nodes.Add(RobotInfo());
            _nodes.Add(BioJetProcessInfo());
        }

        /// <summary>
        /// Overwrites the file if one exists, silently.
        /// </summary>
        public void SaveAsSprayingSystemConfig(string filename)
        {
        }

        public void SaveAsSprayingSystemConfig(string filename, ILogger<AppViewModel> logger)
        {
            ApplyChangesAndSave(filename, logger);
        }

        public void ApplyOnlyToSprayingSystemConfig(ILogger<AppViewModel> logger)
        {
            ApplyChangesOnly(logger);
        }

        public ObservableCollection<ParameterNodes> Nodes
        {
            get { return _nodes; }
        }

        #region Private

        private void ApplyChangesAndSave(string filename)
        {
        }

        private void ApplyChangesAndSave(string filename, ILogger<AppViewModel> logger)
        {
            try
            {
                Unwrap(_nodes, logger);
                _data.Save(filename);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + " Invalid Data Entered");
            }
        }

        private void ApplyChangesOnly(ILogger<AppViewModel> logger)
        {
            try
            {
                Unwrap(_nodes, logger);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + " Invalid Data Entered");
            }
        }

        private void Unwrap(ObservableCollection<ParameterNodes> pNodes)
        {

        }
        private void Unwrap(ObservableCollection<ParameterNodes> pNodes, ILogger<AppViewModel> logger)
        {
            foreach (var pNode in pNodes)
            {
                if (pNode.Name == SprayingSystemConfigGroups.Camera)
                    UnwrapCameraInfo(_data, pNode);

                else if (pNode.Name == SprayingSystemConfigGroups.RPI)
                    UnwrapRpiInfo(_data, pNode);

                else if (pNode.Name == SprayingSystemConfigGroups.Robot)
                    UnwrapRobotInfo(_data, pNode, logger);

                else if (pNode.Name == SprayingSystemConfigGroups.BioJetProcess)
                    UnwrapBioJetProcessInfo(_data, pNode);
            }
        }

        #region Camera

        private ParameterNodes CameraInfo()
        {
            var nodeCamera = new ParameterNodes();
            nodeCamera.Name = SprayingSystemConfigGroups.Camera;
            nodeCamera.Nodes = new ObservableCollection<object>();

            var node = new Node("temp_folder", _data.camera.temp_folder);
            nodeCamera.Nodes.Add(node);

            return nodeCamera;
        }

        private void UnwrapCameraInfo(SprayingSystemConfig json, ParameterNodes pnodes)
        {
            var folder = (Node)pnodes.Nodes[0];
            json.camera.temp_folder = folder.Value;

            IsCameraChanged = folder.IsEdited;
            folder.IsEdited = false;
        }

        public bool IsCameraChanged;

        #endregion

        #region RPI

        private ParameterNodes RpiInfo()
        {
            var nodeRpi = new ParameterNodes();
            nodeRpi.Name = SprayingSystemConfigGroups.RPI;
            nodeRpi.Nodes = new ObservableCollection<object>();

            var node = new Node("ip_address", _data.rpi.ip_address);
            nodeRpi.Nodes.Add(node);

            var node2 = new Node("port", _data.rpi.port.ToString());
            nodeRpi.Nodes.Add(node2);

            return nodeRpi;
        }

        private void UnwrapRpiInfo(SprayingSystemConfig json, ParameterNodes pnodes)
        {
            var ipAddress = (Node)pnodes.Nodes[0];
            json.rpi.ip_address = ipAddress.Value;
            IsRpiChanged = ipAddress.IsEdited;
            ipAddress.IsEdited = false;

            var port = (Node)pnodes.Nodes[1];
            json.rpi.port = int.Parse(port.Value);
            IsRpiChanged = IsRpiChanged || port.IsEdited;
            port.IsEdited = false;
        }

        public bool IsRpiChanged;

        #endregion

        #region Robot

        private ParameterNodes RobotInfo()
        {
            var nodeRobot = new ParameterNodes();
            nodeRobot.Name = SprayingSystemConfigGroups.Robot;
            nodeRobot.Nodes = new ObservableCollection<object>();

            foreach (var location in _data.robot.locations)
            {
                var rootNode = new ParameterNodes();
                rootNode.Name = location.name;
                var nodes = new ObservableCollection<object>();
                rootNode.Nodes = nodes;

                var node = new Node("point", location.point.ToString());
                nodes.Add(node);

                node = new Node("speed", location.speed.ToString());
                nodes.Add(node);

                if (location.motiontype != null)
                {
                    node = new Node("motiontype", location.motiontype);
                    nodes.Add(node);
                }

                nodeRobot.Nodes.Add(rootNode);
            }

            return nodeRobot;
        }

        private void UpdateRobotInfoLocation(SprayingSystemConfig json, RobotLocation location)
        {
        }

        private void UpdateRobotInfoLocation(SprayingSystemConfig json, RobotLocation location, ILogger<AppViewModel> logger)
        {
            foreach (var loc in json.robot.locations)
            {
                if (loc.name == location.name)
                {
                    loc.point = location.point;
                    loc.speed = location.speed;
                    loc.motiontype = location.motiontype;
                    return;
                }
            }
            logger.LogError("Internal error - unable to match robot location name when saving to file. Bad file, do not use it.");
            logger.LogError("error saving settings to file");
        }

        public bool IsRobotEssentialChanged;
        public bool IsRobotMotionsChanged;

        private void UnwrapRobotInfo(SprayingSystemConfig json, ParameterNodes pnodes)
        {
        }

        private void UnwrapRobotInfo(SprayingSystemConfig json, ParameterNodes pnodes, ILogger<AppViewModel> logger)
        {
            IsRobotEssentialChanged = false;
            IsRobotMotionsChanged = false;

            // The order of the locations in the JSON file is not a problem.

            foreach (var i_pnode in pnodes.Nodes)
            {
                var pnode = (ParameterNodes)i_pnode;

                var robotLocation = new RobotLocation();
                robotLocation.name = pnode.Name;

                robotLocation.point = int.Parse(((Node)pnode.Nodes[0]).Value);
                IsRobotEssentialChanged = ((Node)pnode.Nodes[0]).IsEdited || ((Node)pnode.Nodes[0]).IsEdited;
                ((Node)pnode.Nodes[0]).IsEdited = false;

                robotLocation.speed = int.Parse(((Node)pnode.Nodes[1]).Value);
                IsRobotEssentialChanged = ((Node)pnode.Nodes[1]).IsEdited || ((Node)pnode.Nodes[1]).IsEdited;
                ((Node)pnode.Nodes[1]).IsEdited = false;

                if (pnode.Nodes.Count > 2)
                {
                    robotLocation.motiontype = ((Node)pnode.Nodes[2]).Value;
                    IsRobotEssentialChanged = ((Node)pnode.Nodes[2]).IsEdited || ((Node)pnode.Nodes[2]).IsEdited;
                    ((Node)pnode.Nodes[2]).IsEdited = false;
                }

                UpdateRobotInfoLocation(json, robotLocation, logger);
            }
        }

        #endregion

        private ParameterNodes BioJetProcessInfo()
        {
            var nodeProcess = new ParameterNodes();
            nodeProcess.Name = SprayingSystemConfigGroups.BioJetProcess;
            nodeProcess.Nodes = new ObservableCollection<object>();

            var node = new Node("prep_delay", _data.biojet_process.prep_delay.ToString());
            nodeProcess.Nodes.Add(node);

            node = new Node("spray_time", _data.biojet_process.spray_time.ToString());
            nodeProcess.Nodes.Add(node);

            node = new Node("blot_time", _data.biojet_process.blot_time.ToString());
            nodeProcess.Nodes.Add(node);

            node = new Node("clean_time", _data.biojet_process.clean_time.ToString());
            nodeProcess.Nodes.Add(node);

            node = new Node("clean_cycles", _data.biojet_process.clean_cycles.ToString());
            nodeProcess.Nodes.Add(node);

            return nodeProcess;
        }

        private void UnwrapBioJetProcessInfo(SprayingSystemConfig json, ParameterNodes pnodes)
        {
            var prep_delay = (Node)pnodes.Nodes[0];
            if (prep_delay.Value == null)
                json.biojet_process.prep_delay = 0;
            else
                json.biojet_process.prep_delay = int.Parse(prep_delay.Value);
            prep_delay.IsEdited = false;

            var spray_time = (Node)pnodes.Nodes[1];
            if (spray_time.Value == null)
                json.biojet_process.spray_time = 0;
            else
                json.biojet_process.spray_time = int.Parse(spray_time.Value);
            spray_time.IsEdited = false;

            var blot_time = (Node)pnodes.Nodes[2];
            if (blot_time.Value == null)
                json.biojet_process.blot_time = 0;
            else
                json.biojet_process.blot_time = int.Parse(blot_time.Value);
            blot_time.IsEdited = false;

            var clean_time = (Node)pnodes.Nodes[3];
            if (clean_time.Value == null)
                json.biojet_process.clean_time = 0;
            else
                json.biojet_process.clean_time = int.Parse(clean_time.Value);
            clean_time.IsEdited = false;

            var clean_cycles = (Node)pnodes.Nodes[4];
            if (clean_cycles.Value == null)
                json.biojet_process.clean_cycles = 0;
            else
                json.biojet_process.clean_cycles = int.Parse(clean_cycles.Value);
            clean_cycles.IsEdited = false;
        }

        #endregion
    }
}
