using Microsoft.AspNetCore.Mvc;

namespace Communications.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController : ControllerBase
    {
        private readonly int[] _processingDelayRange = [190, 210];

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var delay = new Random().Next(
                _processingDelayRange[0],
                _processingDelayRange[1]);

            await Task.Delay(delay);
            return Ok();
        }
    }
}
