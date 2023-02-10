using Elastic.Apm;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Mvc;

namespace ElasticApmNet6.Controllers;

[ApiController]
[Route("example/error")]
public class ErrorController : ControllerBase
{
    [HttpGet("zero-division")]
    public async Task<IActionResult> ZeroDivision()
    {
        var a = 1;
        var b = 0;

        var c = a / b;

        return Ok(new
        {
            Message = "zero divide error."
        });
    }

    [HttpGet("custom")]
    public async Task<IActionResult> Custom()
    {
        var errorLog = new ErrorLog("twMVC exception")
        {
            Level = "error",
            ParamMessage = "twMVC"
        };

        Agent.Tracer.CaptureErrorLog(errorLog);

        return Ok(new
        {
            Message = "Custom Error"
        });
    }
}