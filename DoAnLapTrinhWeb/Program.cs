using DoAnLapTrinhWeb.Models;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.Data;
using Microsoft.AspNetCore.Identity;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using System.Configuration;

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
   });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RealConnection")));

builder.Services.AddDbContext<DoAnLapTrinhWebDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RealConnection")));

builder.Services.AddDefaultIdentity<AppliactionUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<DoAnLapTrinhWebDbContext>();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt6QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRbQl5hT35bdERjWXtedXA=");

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DashBoard}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
