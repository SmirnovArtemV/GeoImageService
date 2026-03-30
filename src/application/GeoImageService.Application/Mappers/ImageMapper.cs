using GeoImageService.Application.Models.DTO;
using GeoImageService.Application.Models.Images;

namespace GeoImageService.Application.Mappers;

public static class ImageMapper
{
    public static ImageDto ToDto(Image image)
    {
        var dto = new ImageDto(image.Id, image.FilePath, image.GeoTiffFilePath, image.CornersCoordinates, image.TimeStamps);
        return dto;
    }

    public static CutImageDto ToCutDto(Image image)
    {
        var dto = new CutImageDto(image.FilePath, image.GeoTiffFilePath, image.CornersCoordinates);
        return dto;
    }
}