using DoAnLapTrinhWeb.Models;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.Data;
using Microsoft.AspNetCore.Identity;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using System.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OAuth;
using OfficeOpenXml;
using DoAnLapTrinhWeb.Service;
using Microsoft.Extensions.Configuration;
using DoAnLapTrinhWeb.Repositories;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
       options.Events.OnRedirectToAuthorizationEndpoint = (context) =>
       {
           context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
           return Task.CompletedTask;
       };
    
   });


builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
 

    options.Events.OnRedirectToAuthorizationEndpoint = (context) =>
    {
        context.Response.Redirect(context.RedirectUri + "&auth_type=reauthenticate");
        return Task.CompletedTask;
    };
});


builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
    microsoftOptions.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddOptions();                                         
var mailsettings = builder.Configuration.GetSection("MailSettings");  

builder.Services.Configure<MailSettings>(mailsettings);
builder.Services.AddTransient<ISendMailService, SendMailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
AddCookie(options =>
{
    options.Cookie.Name = "Track_Living_Expense";
    options.LoginPath = "/Identity/Account/Login";
});


builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RealConnection")));

builder.Services.AddDbContext<DoAnLapTrinhWebDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RealConnection")));


builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

builder.Services.AddDefaultIdentity<AppliactionUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DoAnLapTrinhWebDbContext>();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt6QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRbQl5hT35bdERjWXtedXA=");

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/DashBoard/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DashBoard}/{action=Index}/{id?}");



app.MapRazorPages();

app.Run();
