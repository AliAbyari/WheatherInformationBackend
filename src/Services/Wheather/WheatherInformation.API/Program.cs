using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;
using WheatherInformation.Application.DTOs.Base;
using WheatherInformation.Infrastructure.Remote.Base;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: $@"{builder.Configuration["SeriLogOptions:LogFilePath"]}{builder.Configuration["SeriLogOptions:LogFileName"]}.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

builder.Services.Configure<OpenWeatherOptions>(builder.Configuration.GetSection("OpenWeatherOptions"));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("SecurityOptions")); 

builder.Services.AddHttpClient();
builder.Services.AddScoped<IRemoteServiceWrapper, RemoteServiceWrapper>();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Weather API",
        Version = "1.01"
    });

    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Basic",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Basic scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Basic"
                            }
                        },
                        new string[] {}
                    }
                });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(app.Configuration["SwaggerAddress1"], "Weather API 1.01");
        c.InjectStylesheet(app.Configuration["SwaggerCssAddress"]);
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
