using Microsoft.AspNetCore.Mvc;

namespace SSCMS.Gather.Controllers
{
    [Route("api/gather/ping")]
    public class PingController : ControllerBase
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public string Get()
        {
            return "pong";
        }
    }
}
