using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RevenueBillingApi.RateLimiting
{
    public class RateLimitMiddleware: ActionFilterAttribute
    {
        private readonly int _limit; // In Numbers

        private readonly TimeSpan _period; // In Minutes

        private readonly ConcurrentDictionary<string, Queue<DateTime>> _clients = new ConcurrentDictionary<string, Queue<DateTime>>();

        public RateLimitMiddleware(int Limit, int Period)
        {
            _limit = Limit;
            _period = TimeSpan.FromMinutes(Period);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path;

            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

            string clientKey = $"{ipAddress}:{path}";
            
            var now = DateTime.Now;

            var client = _clients.GetOrAdd(clientKey, _ => new Queue<DateTime>());

            lock (client)
            {
                while (client.Count > 0 && now - client.Peek() > _period)
                {
                    client.Dequeue();
                }

                if (client.Count >= _limit)
                {
                    context.Result = new ObjectResult("Too Many Requests")
                    {
                        StatusCode = (int)HttpStatusCode.TooManyRequests
                    };
                    return;
                }

                client.Enqueue(now);
            }
        }
      
    }
}