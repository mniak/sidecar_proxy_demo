using Microsoft.AspNetCore.Mvc;

namespace EchoService
{
    public class EchoController : ControllerBase
    {
        [Route("{*url}")]
        public IActionResult Echo(string url)
        {
            return Ok(new
            {
                Url = new
                {
                    Path = Request.Path.ToString(),
                    Query = Request.Query,
                },
                Method = Request.Method,
                Headers = Request.Headers,
            });
        }
    }
}
