using RevenueBillingApp.Middleware;
using Microsoft.Extensions.Primitives;
using RevenueBillingApp.Logging;
using RevenueBillingApp.Utility;
using RevenueBillingApp.Extensions;
using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Models.Session;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddNewtonsoftJson(o =>
{
    o.UseMemberCasing();
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Sessions>();
builder.Services.AddScoped<CommonMethods>();
builder.Services.AddScoped<AuthenticationAccess>();

builder.Services.AddScoped<CookieAuthentication>(); 
builder.Services.AddScoped<RequestClient>();        

builder.Services.AddSingleton<SendEmail>();
builder.Services.AddSingleton<DataEncryptor>();
builder.Services.AddSingleton<RandomStringGenerator>();

builder.Services.AddAuthentication("ASPXAUTH").AddCookie("ASPXAUTH", config =>
{
    config.Cookie.Name = "ASPXAUTH";
    config.LoginPath = "/Login/UserLogin";
    config.AccessDeniedPath = "/Errors/Error?statusCode=403";
    config.LogoutPath = "/Login/Logout";
    config.Cookie.SameSite = SameSiteMode.Strict;
    config.EventsType = typeof(CookieAuthentication);
});


#pragma warning disable ASP0011 // Suggest using builder.Logging over Host.ConfigureLogging or WebHost.ConfigureLogging
builder.Host.ConfigureLogging((context, logging) =>
{
    logging.FileLogger(options =>
    {
        context.Configuration.GetSection("Logging").GetSection("FileLoggerProvider").Bind(options);
    });
});
#pragma warning restore ASP0011 // Suggest using builder.Logging over Host.ConfigureLogging or WebHost.ConfigureLogging



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errors/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseMiddleware<SecurityHeadersMiddleware>();

//app.UseStatusCodePagesWithRedirects("/Errors/Error?statusCode={0}");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

string targetApi = Environment.GetEnvironmentVariable("AM_TARGET_API");

app.Use(async (context, next) =>
{
    var claims = context.User.Claims;
    var UniqueKey = (from c in claims where c.Type == "Guid" select c.Value).FirstOrDefault();
    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();
    var _session = builder.Services.BuildServiceProvider().GetService<Sessions>();
    SessionItems sessionItems = _session.GetSession(UniqueKey, UserID);

    if (sessionItems != null)
    {
        if (sessionItems.userInfo != null)
        {
            DateTime dateTime = DateTime.Now;
            DateTime TokenExpiry = sessionItems.authToken.end_date_time;

            if (TokenExpiry <= dateTime)
            {
                string GetUserPassword = _session.GetUserHashPassword(sessionItems.userInfo.LoginName);
                Tokens token = _session.GetApiToken(sessionItems.userInfo.LoginName, GetUserPassword);
                _session.UpdateSession(UniqueKey, UserID, token);
            }
        }
    }
    if (context.Request.Path.ToString().Contains("/Public/"))
    {
        try
        {
            string[] _path = context.Request.Path.ToString().Split("/");

            context.Response.Headers.Append("Content-Disposition", new StringValues("attachment; " + _path[_path.Length - 1]));
        }
        catch
        {
            context.Response.Headers.Append("Content-Disposition", new StringValues("attachment;"));
        }
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=UserLogin}/{id?}"
);


//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

app.Run();

