using GeoImageSerrvice.Abstractions.Repositories;
using GeoImageService.Application.Helpers;
using GeoImageService.Application.Images;
using GeoImageService.Application.Models.Images;
using GeoImageService.Application.Models.Options;
using GeoImageService.Infrastructure.Contexts;
using GeoImageService.Infrastructure.Repositories;
using GeoImageService.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OSGeo.GDAL;

var builder = WebApplication.CreateBuilder(args);

GdalConfiguration.ConfigureGdal();
Gdal.AllRegister();

builder.Services.AddOptions<DbOptions>().Bind(builder.Configuration.GetSection("DbOptions"));
builder.Services.AddOptions<StorageOptions>().Bind(builder.Configuration.GetSection("StorageOptions"));
builder.Services.AddDbContext<ApplicationContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider.GetRequiredService<IOptions<DbOptions>>().Value.ConnectionString;
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<GdalHelper>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();