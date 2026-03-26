using GeoImageSerrvice.Abstractions.Repositories;
using GeoImageService.Application.Models.Images;
using GeoImageService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GeoImageService.Infrastructure.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly ApplicationContext _context;

    public ImageRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<Image> CreateAsync(CornersCoordinates coordinates, string filePath, string geoTiffPath, CancellationToken cancellationToken)
    {
        var image = new Image
        {
            CornersCoordinates = coordinates,
            FilePath = filePath,
            GeoTiffFilePath = geoTiffPath,
        };
        _context.Images.Add(image);
        await _context.SaveChangesAsync(cancellationToken);
        return image;
    }

    public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken)
    {
        var images = await _context.Images.ToListAsync(cancellationToken);
        return images;
    }
}