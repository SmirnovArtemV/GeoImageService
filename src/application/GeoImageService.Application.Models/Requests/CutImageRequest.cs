using GeoImageService.Application.Models.Images;

namespace GeoImageService.Application.Models.Requests;

public record CutImageRequest(CornersCoordinates CornersCoordinates, TimeStamps TimeStamps);