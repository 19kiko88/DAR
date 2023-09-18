using DesignAuditRise.Service.Implement;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Web;
using Microsoft.AspNetCore.Server.IISIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddControllers();
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});
builder.Services.Configure<AppSettings.OtherSettings>(builder.Configuration.GetSection(AppSettings._OtherSettings));
builder.Services.Configure<AppSettings.DsnServiceParameters>(builder.Configuration.GetSection(AppSettings._DsnServiceParameters));
builder.Services.Configure<AppSettings.PathSettings>(builder.Configuration.GetSection(AppSettings._PathSettings));
builder.Services.AddScoped<IDesignAuditRiseService, DesignAuditRiseService>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IConvertPediaService, DesignAuditRise.Service.OuterService.Implement.ConvertPediaService>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IDesignAuditRiseService, DesignAuditRise.Service.OuterService.Implement.DesignAuditRiseService>();



/*CORS*/
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("allowCors",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });
}

/*AutoMapper*/
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    //套用CORS
    app.UseCors("allowCors");
}
app.UseSpaStaticFiles();
app.UseEndpoints(endpoints =>
{//不管有無[Authenrization]，都會進行授權驗證
    endpoints.MapControllers().RequireAuthorization();
});
app.MapControllers();
app.UseSpaStaticFiles();
app.UseSpa(spa =>
{
    spa.Options.SourcePath = $"wwwroot";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});


app.Run();
