using Elastic.Apm.NetCoreAll;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

//Configure Serilog to ElasticSearch.
var formatter = new EcsTextFormatter(new EcsTextFormatterConfiguration()
    .MapCustom((ecsDoc, log) =>
    {
        ecsDoc.Service = new Service { Name = builder.Configuration["Elastic:ServiceName"] };
        return ecsDoc;
    }));

builder.Host.UseSerilog((context, services, config) =>
    config
        //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.WithElasticApmCorrelationInfo()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                new Uri(context.Configuration["Elastic:ServerUrl"]))
            {
                IndexFormat = context.Configuration["Elastic:IndexFormat"],
                CustomFormatter = formatter,
                ModifyConnectionSettings = x => x
                    .ApiKeyAuthentication(context.Configuration["Elastic:ApiKeyId"], context.Configuration["Elastic:ApiKey"])
                    .ConnectionLimit(-1)
            }
        )
        .WriteTo.Console(formatter)
);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure elastic apm agent enablement
app.UseAllElasticApm(app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


