using DesignAuditRise.Service.Implement;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Web;
using ElmahCore.Mvc;
using ElmahCore;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Reflection;
using Serilog;
using DesignAuditRise.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("seri-log.config.json").Build())
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
//�w�]�ϥ�Windows����
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
//�]�w�䴩 Negotiate, �]�tKerberos �� NTLM ������Ҥ覡
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c => {
    // Ū�� XML �ɮײ��� API ����
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});
builder.Services.Configure<AppSettingsInfoModel.ConnectionStrings>(builder.Configuration.GetSection(AppSettingsInfoModel._ConnectionStrings));
builder.Services.Configure<AppSettingsInfoModel.OtherSettings>(builder.Configuration.GetSection(AppSettingsInfoModel._OtherSettings));
builder.Services.Configure<AppSettingsInfoModel.DsnServiceParameters>(builder.Configuration.GetSection(AppSettingsInfoModel._DsnServiceParameters));
builder.Services.Configure<AppSettingsInfoModel.PathSettings>(builder.Configuration.GetSection(AppSettingsInfoModel._PathSettings));
builder.Services.Configure<AppSettingsInfoModel.UrlSettings>(builder.Configuration.GetSection(AppSettingsInfoModel._UrlSettings));

builder.Services.AddScoped<IDesignAuditRiseService, DesignAuditRiseService>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<IProtobufService, ProtobufService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IZipService, ZipService>();
builder.Services.AddScoped<IFileWrapper, FileWrapper>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IOraOuterService, DesignAuditRise.Service.OuterService.Implement.OraOuterService>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IConvertPediaOuterService, DesignAuditRise.Service.OuterService.Implement.ConvertPediaOterService>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IDesignCompareOuterService, DesignAuditRise.Service.OuterService.Implement.DesignCompareOterService>();
builder.Services.AddScoped<DesignAuditRise.Service.OuterService.Interface.IProcessDsnOuterService, DesignAuditRise.Service.OuterService.Implement.ProcessDsnOuterService>();
builder.Services.AddScoped(typeof(AllowedIpAttribute));





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

//Ref�Ghttps://github.com/ElmahCore/ElmahCore
builder.Services.AddElmah<XmlFileErrorLog>(options =>
{
    options.Path = "ElmahWebLogs"; //web log path�Ghttps://localhost:7162/ElmahWebLogs/errors
    options.LogPath = "./logs/ElmahXmlLogs";
    var allowedUsers = new string[] { "ASUS\\Abel_Hsu", "ASUS\\Bruenor_Hsu", "ASUS\\Homer_Chen" };
    options.OnPermissionCheck = Context =>
    {
        if (Context.User.Identity.IsAuthenticated && allowedUsers.Contains(Context.User.Identity.Name, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }
        else
        {
            return false;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    //�M��CORS
    app.UseCors("allowCors");
}
app.UseSpaStaticFiles();

//ELMAH must bwtween UseAuthorization and UseEndpoints
app.UseElmah();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        //options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseEndpoints(endpoints =>
{//���ަ��L[Authenrization]�A���|�i����v����
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
