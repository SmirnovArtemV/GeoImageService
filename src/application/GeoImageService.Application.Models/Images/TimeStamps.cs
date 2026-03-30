namespace GeoImageService.Application.Models.Images;

public class TimeStamps
{
    public double? Start { get; init; }
    public double? End { get; init; }

    public TimeStamps(double start, double end)
    {
        Start = start;
        End = end;
    }
    
    public TimeStamps(){}
}