using System.Diagnostics;

namespace GOOM.ERC.CorrelationIds.CoreApi
{
    public class TimingMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger<TimingMiddleware> _logger;

        public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("Starting request.");

            var next = _next(context);

            sw.Stop();
            _logger.LogInformation($"Request complete after {sw.ElapsedMilliseconds}ms");

            return next;
        }
    }
}
