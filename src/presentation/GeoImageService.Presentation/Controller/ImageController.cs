using GeoImageService.Application.Images;
using GeoImageService.Application.Models.Images;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GeoImageService.Presentation.Controller;

[ApiController]
public class ImageController : ControllerBase
{
    private readonly ImageService _service;

    public ImageController(ImageService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("api/image")]
    public async Task<ActionResult<IEnumerable<Image>>> GetAllImagesInfos(CancellationToken ct)
    {
        var results = await _service.GetAllAsync(ct);
        return Ok(results);
    }

    [HttpPost]
    [Route("api/image")]
    public async Task<ActionResult<Image>> SaveImage(IFormFile photo, IFormFile kmlFile, [FromQuery] string filename,
        CancellationToken ct)
    {
        var image = await _service.CreateAsync(photo, kmlFile, filename, ct);
        return Ok(image);
    }

    [HttpGet]
    [Route("api/image/get-intersection-images")]
    public async Task<ActionResult<IEnumerable<Image>>> GetImageInfos([FromQuery] CornersCoordinates rectangle, CancellationToken ct)
    {
        return await _service.GetImagesByCoordinateIntersection(rectangle, ct);
    }

    [HttpPost]
    [Route("api/image/cut")]
    public async Task<ActionResult<Image>> CutImage([FromBody] CornersCoordinates rectangle, CancellationToken ct)
    {
        return await _service.CutImage(rectangle, ct);
    }
}