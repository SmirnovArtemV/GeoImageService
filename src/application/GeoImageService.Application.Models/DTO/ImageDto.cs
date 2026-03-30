using GeoImageService.Application.Models.Images;

namespace GeoImageService.Application.Models.DTO;

public record ImageDto(
    long Id,
    string FilePath,
    string GeoTiffFilePath,
    CornersCoordinates CornersCoordinates,
    TimeStamps TimeStamps);