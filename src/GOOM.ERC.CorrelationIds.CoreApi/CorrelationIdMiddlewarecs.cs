using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

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
            if (context.Request.Headers.TryGetValue(CorrelationHeader, out StringValues correlationId))
            {
                context.TraceIdentifier = correlationId;
            }

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
