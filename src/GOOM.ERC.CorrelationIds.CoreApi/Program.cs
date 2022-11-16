using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using System.Reflection;

namespace GOOM.ERC.CorrelationIds.CoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            ConfigureLogging(builder.Configuration);

            builder.Host.UseSerilog((context, services, configuration) => ConfigureLogging(configuration, builder.Configuration));

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<TimingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureLogging(IConfiguration configuration)
        {
            Log.Logger = ConfigureLogging(new LoggerConfiguration(), configuration)
                .CreateLogger();
        }

        private static LoggerConfiguration ConfigureLogging(LoggerConfiguration loggingConfiguration, IConfiguration configuration)
        {
           return loggingConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearch:Url"]))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = "logs-{0:yyyy.MM.dd}",
                    BatchAction = ElasticOpType.Create,
                    TypeName = null,
                    ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "changeme")
                })
                .ReadFrom.Configuration(configuration);
        }
    }
}