using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariff.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//builder.Services.AddDbContext<ApplicationDBContext>();
builder.Services.AddDbContextFactory<ApplicationDBContext>(opt =>
    opt.UseSqlite($"DataSource = AztecTariff.db;"));
// Add Telerik Blazor server side services
builder.Services.AddTelerikBlazor();

builder.Services.AddSingleton<Settings>();

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<SalesAreaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
