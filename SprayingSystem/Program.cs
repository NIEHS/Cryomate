using SprayingSystem.ViewModels;

var builder = WebApplication.CreateBuilder(args);

var inMemoryLogProvider = new InMemoryLogProvider();
builder.Logging.AddProvider(inMemoryLogProvider);
builder.Services.AddSingleton(inMemoryLogProvider);  // Make it available for DI
builder.Services.AddSingleton<SerialPortService>();

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<AppViewModel>();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

var serialService = app.Services.GetRequiredService<SerialPortService>();
serialService.Start();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<LogHub>("/logHub"); // Ensure LogHub is mapped
app.MapHub<SimulationHub>("/simulationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
