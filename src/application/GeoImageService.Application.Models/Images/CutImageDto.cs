namespace GeoImageService.Application.Models.Images;

public record CutImageDto(string FilePath, string GeoTiffFilePath, CornersCoordinates CornersCoordinates);