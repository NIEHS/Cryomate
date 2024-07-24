using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SprayingSystem.Models;
using SprayingSystem.ViewModels;
using System.IO;
using System.IO.Ports;

namespace SprayingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RobotController : ControllerBase
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly AppViewModel _appViewModel;
        private readonly SerialPortService _serialPortService;
        private readonly RecordingService _recordingService;

        public RobotController(AppViewModel appViewModel, IHubContext<LogHub> hubContext, SerialPortService serialPortService, RecordingService recordingService)
        {
            _appViewModel = appViewModel;
            _hubContext = hubContext;
            _serialPortService = serialPortService;
            _recordingService = recordingService;
        }

        // Robot Actions

        [HttpPost("PowerDown")]
        public async Task<IActionResult> PowerDown()
        {
            var logMessage = "Powering Down...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.PowerDownCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("ConnectCmd")]
        public async Task<IActionResult> ConnectCmd()
        {
            var logMessage = "Connecting Robot...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.ConnectCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("HighPowerOn")]
        public async Task<IActionResult> HighPowerOn()
        {
            var logMessage = "Turning High Power On...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.HighPowerOnCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("MoveToLoadTweezers")]
        public async Task<IActionResult> MoveToLoadTweezers()
        {
            var logMessage = "Executing MoveToLoadTweezers...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.MoveToLoadTweezersCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("MoveToSprayPosition")]
        public async Task<IActionResult> MoveToSprayPosition()
        {
            var logMessage = "Executing MoveToSprayPosition...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.MoveToSprayPositionCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        // App Actions

        [HttpPost("RobotManagerPresenter")]
        public async Task<IActionResult> RobotManagerPresenter()
        {
            var logMessage = "Opening Robot Manager...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.RobotManagerPresenterCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("IoMonitorPresenter")]
        public async Task<IActionResult> IoMonitorPresenter()
        {
            var logMessage = "Opening I/O Monitor...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.IoMonitorPresenterCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("EditConfigSettings")]
        public async Task<IActionResult> EditConfigSettings()
        {
            var logMessage = "Editing Config Settings...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.EditConfigSettingsCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("PointTeachingPresenter")]
        public async Task<IActionResult> PointTeachingPresenter()
        {
            var logMessage = "Opening Point Teaching Presenter...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.PointTeachingPresenterCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("ControllerToolsPresenter")]
        public async Task<IActionResult> ControllerToolsPresenter()
        {
            var logMessage = "Opening Controller Tools Presenter...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.ControllerToolsPresenterCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        /*

        [HttpPost("CleanSprayer")]
        public async Task<IActionResult> CleanSprayer()
        {
            var logMessage = "Cleaning Sprayer...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RpiViewModel.CleanSprayerCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("ConnectToRPI")]
        public async Task<IActionResult> ConnectToRPI()
        {
            var logMessage = "Connecting to RPI...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RpiViewModel.ConnectCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("PlayVideo")]
        public async Task<IActionResult> PlayVideo()
        {
            var logMessage = "Playing Video...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RpiViewModel.PlayVideoCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }
        */

        // Camera Actions

        [HttpPost("TakePicture")]
        public async Task<IActionResult> TakePicture()
        {
            var logMessage = "Taking Picture...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.CameraViewModel.TakePictureCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        [HttpPost("SavePicture")]
        public async Task<IActionResult> SavePicture()
        {
            var logMessage = "Saving Picture...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.CameraViewModel.SavePictureCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }

        // Serial Port (Arduino) Actions
        [HttpPost("StartSerialPort")]
        public IActionResult StartSerialPort([FromBody] string comPort)
        {
            try
            {
                if (_serialPortService.Start(comPort))
                    return Ok();
                else
                    return BadRequest("Could not open COM port. Check if the port is already in use or if the device is connected properly.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("StopSerialPort")]
        public IActionResult StopSerialPort()
        {
            _serialPortService.Stop();
            return Ok();
        }

        [HttpGet("AvailableSerialPorts")]
        public IActionResult GetAvailablePorts()
        {
            var ports = SerialPort.GetPortNames();
            _appViewModel.LogAction("Available ports: " + string.Join(", ", ports));
            return Ok(ports);
        }

        // Recording Actions
        [HttpPost("StartRecording")]
        public IActionResult StartRecording()
        {
            _recordingService.StartRecording();
            return Ok();
        }

        [HttpPost("StopRecording")]
        public IActionResult StopRecording()
        {
            _recordingService.StopRecording();
            return Ok();
        }

        [HttpGet("ListRecordings")]
        public IActionResult ListRecordings()
        {
            var recordings = _recordingService.GetAvailableRecordings();
            var recordingFiles = recordings.Select(Path.GetFileName).ToArray();
            return Ok(recordingFiles);
        }

        [HttpGet("DownloadRecording/{fileName}")]
        public IActionResult DownloadRecording(string fileName)
        {
            var filePath = _recordingService.GetRecordingFilePath(fileName);
            if (System.IO.File.Exists(filePath))
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/octet-stream", fileName);
            }
            return NotFound();
        }


        [HttpPost("UpdateGridInfo")]
        public IActionResult UpdateGridInfo([FromBody] GridInfo gridInfo)
        {
            if (ModelState.IsValid)
            {
                _appViewModel.GridViewModel.GridBoxName = gridInfo.BoxName;
                _appViewModel.GridViewModel.SampleName = gridInfo.SampleName;
                _appViewModel.GridViewModel.GridPosition = gridInfo.Position;
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPost("StoreGrid")]
        public IActionResult StoreGrid()
        {
            _appViewModel.GridViewModel.StoreGridCmd.Execute(null);
            return Ok();
        }

        [HttpPost("ConnectToCamera")]
        public async Task<IActionResult> ConnectToCamera()
        {
            var logMessage = "Connecting to Camera...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.CameraViewModel.ConnectCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
            return Ok();
        }


        // Process Options Actions

        // Get current process options
        [HttpGet("ProcessOptions")]
        public IActionResult GetProcessOptions()
        {
            // Assuming ProcessOptionsViewModel contains properties that can be safely sent to the client
            return Ok(_appViewModel.ProcessOptionsViewModel);
        }

        // Update all process options
        [HttpPost("UpdateProcessOptions")]
        public async Task<IActionResult> UpdateProcessOptions([FromBody] ProcessOptionsViewModel options)
        {
            if (ModelState.IsValid)
            {
                // Assuming you would add a method in ViewModel to update all properties or manually set them here.
                _appViewModel.ProcessOptionsViewModel.RecordSpray = options.RecordSpray;
                _appViewModel.ProcessOptionsViewModel.RpiRecordSpray = options.RpiRecordSpray;
                _appViewModel.ProcessOptionsViewModel.Spray = options.Spray;
                _appViewModel.ProcessOptionsViewModel.Blot = options.Blot;
                _appViewModel.ProcessOptionsViewModel.Blot_BackBlot = options.Blot_BackBlot;
                _appViewModel.ProcessOptionsViewModel.Blot_FrontBlot = options.Blot_FrontBlot;
                _appViewModel.ProcessOptionsViewModel.SonicateTweezers = options.SonicateTweezers;

                _appViewModel.LogAction("Process options updated."); // Log action if needed
                await _hubContext.Clients.All.SendAsync("ReceiveLog", "Process options updated.");
                return Ok();
            }
            return BadRequest(ModelState);
        }

    }
}
