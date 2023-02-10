using Elastic.Apm;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace ElasticApmNet7.Controllers;

[ApiController]
[Route("example/trace")]
public class TraceController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _factory;

    public TraceController(ILogger logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    [HttpGet("span/simple")]
    public async Task<IActionResult> Simple()
    {
        var transaction = Agent.Tracer.CurrentTransaction;
        var span1 = transaction.StartSpan("Span1", "mssql");
        await Task.Delay(1000);
        span1.End();

        var span2 = span1.StartSpan("Span2", "local");
        await Task.Delay(2000);
        span2.End();

        var span3 = span2.StartSpan("Span3", "http");
        await Task.Delay(3000);
        span3.End();

        return Ok(new
        {
            Message = "span example with simple delay."
        });
    }
    
    [HttpGet("http")]
    public async Task<IActionResult> Http()
    {
        var client = _factory.CreateClient();

        await client.GetAsync("https://google.com");
        await client.GetAsync("https://ipinfo.io");
        await client.GetAsync("https://networkcalc.com/api/ip/192.168.1.1/24");

        return Ok(new
        {
            Message = "http client tracing sample"
        });
    }
    
    [HttpGet("logger")]
    public async Task<IActionResult> Logger()
    {
        
        _logger.Information("log info in trace...");
        _logger.Warning("log warning in trace...");
        _logger.Error("log error in trace...");

        return Ok(new
        {
            Message = "Trace & Logger"
        });
    }
    
    [HttpGet("span/error")]
    public async Task<IActionResult> Error()
    {
        var transaction = Agent.Tracer.CurrentTransaction;
        var span1 = transaction.StartSpan("Span1", "mssql");
        await Task.Delay(1000);
        span1.End();

        var span2 = span1.StartSpan("Span2", "local");
        await Task.Delay(2000);

        var a = 1;
        var b = 0;

        try
        {
            var c = a / b;
        }
        catch (Exception e)
        {
            span2.CaptureException(e);
            throw;
        }
        finally
        {
            span2.End();
        }

        var span3 = span2.StartSpan("Span3", "http");
        await Task.Delay(3000);
        span3.End();

        return Ok(new
        {
            Message = "span example with simple delay."
        });
    }
}