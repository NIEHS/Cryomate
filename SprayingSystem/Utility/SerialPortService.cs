using System;
using System.IO.Ports;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class SerialPortService
{
    private readonly SerialPort _serialPort;
    private readonly IHubContext<LogHub> _hubContext;

    public SerialPortService(IHubContext<LogHub> hubContext)
    {
        _hubContext = hubContext;
        _serialPort = new SerialPort("COM3", 9600); // Adjust the COM port and baud rate as needed
        _serialPort.DataReceived += SerialPortDataReceived;
    }

    public void Start()
    {
        _serialPort.Open();
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = _serialPort.ReadLine();
        _hubContext.Clients.All.SendAsync("ReceiveMessage", data);
    }

    public void Stop()
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }
}
