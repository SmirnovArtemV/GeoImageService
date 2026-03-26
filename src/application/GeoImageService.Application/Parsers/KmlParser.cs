using System.Globalization;
using System.Xml;
using GeoImageService.Application.Models.Images;

namespace GeoImageService.Application.Parsers;

public static class KmlParser
{
    public static CornersCoordinates Parse(XmlDocument xmlDocument)
    {
        var coordinates = xmlDocument.GetElementsByTagName("coordinates");
        var coordinateNode = coordinates[0];
        var innerText = coordinateNode?.InnerText.Trim();

        var points = innerText?.Split(' ')
            .Select(p =>
            {
                var parts = p.Split(',');
                var longitude = double.Parse(parts[0], CultureInfo.InvariantCulture);
                var latitude = double.Parse(parts[1], CultureInfo.InvariantCulture);
                return new Coordinate(latitude, longitude);
            }).ToList();
        var cornersCoordinates = new CornersCoordinates(points?[0], points?[1], points?[2], points?[3]);
        return cornersCoordinates;
    }
}