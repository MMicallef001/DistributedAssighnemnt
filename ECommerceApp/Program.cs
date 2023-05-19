using ECommerceApp.DataAccess;
using Google.Api;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

string projectId = "distributedprogramming-386320";


builder.Services.AddScoped<PubSubRepositary>(provider => new PubSubRepositary(projectId));
builder.Services.AddScoped<ShippingPubSubRepo>(provider => new ShippingPubSubRepo(projectId));


var environment = builder.Services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

string credential_path = System.IO.Path.Combine(environment.ContentRootPath, "distributedprogramming-386320-7cd52fa89f04.json");

System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
        });

builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder
            .WithOrigins("https://localhost:7011/")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); 
        });
});


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

app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
