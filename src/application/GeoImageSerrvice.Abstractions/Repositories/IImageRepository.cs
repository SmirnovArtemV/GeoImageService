using GeoImageService.Application.Models.Images;

namespace GeoImageSerrvice.Abstractions.Repositories;

public interface IImageRepository
{
    Task<Image> CreateAsync(CornersCoordinates coordinates, string filePath, string geoTiffPath, TimeStamps timeStamps, CancellationToken cancellationToken);
    Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken);
}