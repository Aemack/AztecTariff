using AztecTariff;
using AztecTariff.Models;
using AztecTariff.Services;
using AztecTariffModels.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<TariffDatabaseContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("TariffDatabase"), b => b.MigrationsAssembly("AztecTariff")));
builder.Services.AddControllersWithViews();

builder.Services.AddTelerikBlazor();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<Settings>();
} else
{
    builder.Services.AddSingleton<Settings>();
}
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<SalesAreaService>();
builder.Services.AddScoped<PDFDataService>();
builder.Services.AddScoped<HttpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseSwagger();
    app.UseSwaggerUI();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
Startup.SetUpFolders();