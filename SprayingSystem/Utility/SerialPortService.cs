using System;
using System.IO.Ports;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class SerialPortService
{
    private SerialPort _serialPort;
    private readonly IHubContext<LogHub> _hubContext;
    private readonly RecordingService _recordingService;

    public SerialPortService(IHubContext<LogHub> hubContext, RecordingService recordingService)
    {
        _hubContext = hubContext;
        _recordingService = recordingService;
    }

    public bool Start(string comPort)
    {
        if (_serialPort?.IsOpen == true)
            Stop();

        _serialPort = new SerialPort(comPort, 115200);
        _serialPort.DataReceived += SerialPortDataReceived;

        try
        {
            _serialPort.Open();
            SendCurrentTime();
            return true;
        }
        catch (Exception ex)
        {
            _hubContext.Clients.All.SendAsync("ReceiveLog", "Failed to open COM port: " + ex.Message);
            return false;
        }
    }

    private void SendCurrentTime()
    {
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _serialPort.WriteLine(currentUnixTime.ToString());
    }

    public void Stop()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (!_serialPort.IsOpen)
        {
            return;
        }

        try
        {
            string data = _serialPort.ReadLine();

            // Parse the sensor data directly
            var parts = data.Split(',');
            if (parts.Length == 5)
            {
                var sensorData = new SensorData
                {
                    Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[0])).ToString("yyyy-MM-dd HH:mm:ss"),
                    Temperature = double.Parse(parts[1]),
                    Humidity = double.Parse(parts[2]),
                    Pressure = double.Parse(parts[3]),
                    IrSensorState = int.Parse(parts[4])
                };
                _hubContext.Clients.All.SendAsync("ReceiveSensorData", sensorData);
                var logEntry = $"{sensorData.Time},{sensorData.Temperature},{sensorData.Humidity},{sensorData.Pressure},{sensorData.IrSensorState}";
                _recordingService.WriteSensorData(logEntry);
            }
            else
            {
                _hubContext.Clients.All.SendAsync("ReceiveLog", "Received data in unexpected format: " + data);
            }
        }
        catch (InvalidOperationException)
        {
            // The port is closed, ignore this exception
        }
        catch (Exception ex)
        {
            _hubContext.Clients.All.SendAsync("ReceiveLog", "Error parsing sensor data: " + ex.Message);
        }
    }
}

public class SensorData
{
    public string Time { get; set; } // Human-readable time
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public int IrSensorState { get; set; } // IR sensor state
}
