using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace GOOM.ERC.CorrelationIds.CoreApi
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationHeader = "X-Erc-Correlation-ID";
        private const string ParentHeader = "X-Erc-Parent-ID";
        private const bool IncludeInResponse = false;

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationHeader, out StringValues correlationId))
            {
                correlationId = new StringValues(Guid.NewGuid().ToString());
            }

            if (!context.Request.Headers.TryGetValue(ParentHeader, out StringValues parentRequestId))
            {
                parentRequestId = new StringValues();
            }

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ParentRequestId", parentRequestId))
            {
                if (IncludeInResponse)
                {
                    // apply the correlation ID to the response header for client side tracking
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers.Add(CorrelationHeader, new[] { context.TraceIdentifier });
                        return Task.CompletedTask;
                    });
                }

                return _next(context);
            }
        }
    }
}
