using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaoProject1.Areas.Identity.Data;
using TpaoProject1.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DatabaseContextConnection") ?? throw new InvalidOperationException("Connection string 'DatabaseContextConnection' not found.");
var cultureInfo = new CultureInfo("en-US"); // Burada istedi�iniz dil ve b�lgeyi belirtebilirsiniz
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US"); // �stedi�iniz dil ve b�lgeyi burada belirtebilirsiniz
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Other options you might have
    options.SignIn.RequireConfirmedAccount = false;
})
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<DatabaseContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});
builder.Services.AddRazorPages();

builder.Services.AddHttpClient<MapsGeocodingService>();

builder.Services.AddAuthentication(); // If not already added
builder.Services.AddAuthorization(); // If not already added

// Add email sender service using Gmail SMTP
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Configure the email sender options (Gmail example)
builder.Services.Configure<EmailSenderOptions>(options =>
{
    options.Host = "smtp.gmail.com";
    options.Port = 587;
    options.EnableSsl = true;
    options.UserName = "melisayuncu.my@gmail.com"; // Replace with your Gmail email
    options.Password = "lpcc lstm yego kqmx"; // Replace with your Gmail application password
});

//builder.Services.ConfigureDbContext(builder.Configuration);
var app = builder.Build();

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
app.UseSession();
app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();





app.Run();


