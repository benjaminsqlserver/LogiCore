using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

using LogiCore.Client;
using LogiCore.Client.Services.Shipments;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "LogiCoreTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register client-side shipment service (calls /api/shipments)
builder.Services.AddScoped<IShipmentService, ShipmentHttpService>();

var host = builder.Build();
await host.RunAsync();