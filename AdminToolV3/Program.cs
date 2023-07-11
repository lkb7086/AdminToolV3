using AdminToolV3.CommonClass;
using MySqlConnector.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// TODO: Session
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.MaxValue;
});

var app = builder.Build();

// TODO
CommonDefine.Configuration = app.Configuration;

// TODO: MySqlConnectorLog
MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// TODO: Session
app.UseSession();

app.Run();
