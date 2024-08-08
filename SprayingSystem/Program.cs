using SprayingSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var inMemoryLogProvider = new InMemoryLogProvider();
builder.Logging.AddProvider(inMemoryLogProvider);
builder.Services.AddSingleton(inMemoryLogProvider);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<AppViewModel>();
builder.Services.AddSingleton<SerialPortService>();
builder.Services.AddSingleton<RecordingService>();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Ensure SerialPortService and RecordingService are initialized
var serialService = app.Services.GetRequiredService<SerialPortService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve static files from the "camera-recordings" directory in the user's Documents folder
var cameraRecordingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "camera-recordings");
if (!Directory.Exists(cameraRecordingsPath))
{
    Directory.CreateDirectory(cameraRecordingsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(cameraRecordingsPath),
    RequestPath = "/Documents/camera-recordings"
});

app.UseRouting();

app.UseAuthorization();

app.MapHub<LogHub>("/logHub"); // Ensure LogHub is mapped
app.MapHub<SimulationHub>("/simulationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
