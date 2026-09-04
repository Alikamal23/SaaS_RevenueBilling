using RevenueBillingApi.Extensions;
using RevenueBillingApi.Logging;
using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Repository;
using RevenueBillingApi.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddNewtonsoftJson(o =>
{
    o.UseMemberCasing();
});
builder.Services.AddSingleton<CommonMethods>();
builder.Services.AddSingleton<IJWTManagerRepository, JWTManagerRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<UserDAL>();
builder.Services.AddSingleton<RevenueBillingApi.Context.DataAccessLayer>();
builder.Services.AddSingleton<SendEmail>();
builder.Services.AddSingleton<DataEncryptor>();
builder.Services.AddSingleton<RandomStringGenerator>();

builder.Host.ConfigureLogging((context, logging) =>
{
    logging.FileLogger(options =>
    {
        context.Configuration.GetSection("Logging").GetSection("FileLoggerProvider").Bind(options);
    });
});
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("AM_JWT_KEY"));
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "UserID",
        SaveSigninToken = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("AM_JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("AM_JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(Key),
        ClockSkew = TimeSpan.Zero
    };

    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Path == "/signalr" || context.Request.Path == "/signalr/negotiate")
            {
               // return new OAuthSignalRConnection().RequestToken(context);
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
            }
            return Task.CompletedTask;
        }
    };
});



builder.Services.AddCors(o =>
{
    o.AddPolicy("_AllowedWebAPI",
        policy =>
        {
            policy.WithOrigins(Environment.GetEnvironmentVariable("AM_VIEWURL")).AllowAnyHeader().AllowAnyMethod().AllowCredentials().Build();
        });
});

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
    o.KeepAliveInterval = TimeSpan.FromDays(1);
}).AddNewtonsoftJsonProtocol(o =>
{
    o.PayloadSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
    {
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
    };
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Version V1");
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("_AllowedWebAPI");

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
app.Run();
