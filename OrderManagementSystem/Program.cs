using Microsoft.EntityFrameworkCore;
using OrderManagementSystem;
using OrderManagementSystem_DAL.DependencyInjection;
using OrderManagementSystem_DAL.Entities;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCommonRepository();

builder.Services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 👇 ADD THIS
RotativaConfiguration.Setup(
    builder.Environment.WebRootPath,
    "Rotativa"
);

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
    pattern: "{controller=OrderList}/{action=Index}/{id?}");

app.Run();
