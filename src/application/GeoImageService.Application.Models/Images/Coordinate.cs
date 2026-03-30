namespace GeoImageService.Application.Models.Images;

public class Coordinate
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public Coordinate(){}
}