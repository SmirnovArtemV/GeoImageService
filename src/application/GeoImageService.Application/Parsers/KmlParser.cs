using System.Globalization;
using System.Xml;
using GeoImageService.Application.Models.Images;

namespace GeoImageService.Application.Parsers;

// Почему статик? Спрятать за интерфейс и сделать не статик. Реализаций парсера кмл может быть много.
public static class KmlParser
{
    // TryDo паттерн выглядит так:
    // bool TryDo(..., out result)
    // в твоем случае - TryParseCoordinates(XmlDocument xmlDocument, out CornerCoordinates coordinates)
    // так значительно проще использовать (нет проверок на null) и меньше вероятность null reference exception
    // Также стоит рассмотреть перегрузку метода - принимать сразу Stream - клиентский код вроде так использует везде
    public static CornersCoordinates? TryParseCoordinates(XmlDocument xmlDocument)
    {
        try
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
        catch
        {
            return null;
        }
    }

    public static TimeStamps? TryParseTimeStamps(XmlDocument xmlDocument)
    {
        try
        {
            var node = xmlDocument.GetElementsByTagName("timestamps");
            var timeStampNode = node[0];
            var innerText = timeStampNode?.InnerText.Trim();

            var timeStamps = innerText?.Split(',').Select(t => double.Parse(t, CultureInfo.InvariantCulture)).ToList();
            if (timeStamps != null)
            {
                var result = new TimeStamps(timeStamps[0], timeStamps[1]);
                return result;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
