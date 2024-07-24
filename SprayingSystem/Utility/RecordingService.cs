using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Windows.Markup;

public class RecordingService
{
    private readonly IHubContext<LogHub> _hubContext;
    private string _currentFilePath;
    private bool _isRecording;

    public RecordingService(IHubContext<LogHub> hubContext)
    {
        _hubContext = hubContext;
        _isRecording = false;
    }

    public void StartRecording()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sensor-recordings");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _currentFilePath = Path.Combine(directory, $"recording_{timestamp}.txt");
        File.AppendAllText(_currentFilePath, "TIME,TEMPERATURE,HUMIDITY,PREASURE,PROXIMITY" + Environment.NewLine);
        _isRecording = true;
    }

    public void StopRecording()
    {
        _isRecording = false;
    }

    public void WriteSensorData(string data)
    {
        if (_isRecording && !string.IsNullOrEmpty(_currentFilePath))
        {
            File.AppendAllText(_currentFilePath, data + Environment.NewLine);
        }
    }

    public string[] GetAvailableRecordings()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sensor-recordings");
        if (Directory.Exists(directory))
        {
            return Directory.GetFiles(directory);
        }
        return Array.Empty<string>();
    }

    public string GetRecordingFilePath(string fileName)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sensor-recordings");
        return Path.Combine(directory, fileName);
    }
}
