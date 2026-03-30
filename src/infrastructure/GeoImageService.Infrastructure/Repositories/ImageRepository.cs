using GeoImageSerrvice.Abstractions.Repositories;
// Слой БД узнал про слой Application - зависимость должна быть в другую сторону
// так произошло, потому что нет выделенных ДТО для БД
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

    public async Task<Image> CreateAsync(CornersCoordinates coordinates, string filePath, string geoTiffPath, TimeStamps timeStamps, CancellationToken cancellationToken)
    {
        var image = new Image
        {
            CornersCoordinates = coordinates,
            FilePath = filePath,
            GeoTiffFilePath = geoTiffPath,
            TimeStamps = timeStamps,
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
