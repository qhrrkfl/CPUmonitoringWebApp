using MonitoringService.MonitorServer;
using MonitorWebApp.Middleware;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddConnections();
builder.Services.AddSingleton<IMonitorServer>(x => new MonitorServer(6120));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(5),
    AllowedOrigins = { "*" }
};

//webSocketOptions.AllowedOrigins.Add("https://client.com");
app.UseWebSockets(webSocketOptions);

app.UseMiddleware<WebSocketMiddleWare>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}");
});
app.MapRazorPages();

app.Run();
