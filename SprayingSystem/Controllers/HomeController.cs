using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SprayingSystem.Models;
using SprayingSystem.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace SprayingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly AppViewModel _appViewModel;

        public HomeController(AppViewModel appViewModel, IHubContext<LogHub> hubContext)
        {
            _appViewModel = appViewModel;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var logs = _appViewModel.GetLogs();
            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> ConnectCmd()
        {
            var logMessage = "Connecting Robot...";
            _appViewModel.LogAction(logMessage);
            _appViewModel.RobotViewModel.ConnectCmd.Execute(null);

            await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);

            return RedirectToAction("Logs");
        }

        public IActionResult Logs()
        {
            var logs = _appViewModel.GetLogs();
            return View(logs);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
