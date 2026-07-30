using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;


[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
public class TranscriptsController : ControllerBase
{

    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript(
        [FromBody] object? request)
    {
        return Ok();
    }
}