using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Identity.Api.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services) 
        {
          services.AddRateLimiter(options =>
          {
              options.AddFixedWindowLimiter("auth", policy =>
              {
                  policy.PermitLimit = 5;              
                  policy.Window = TimeSpan.FromMinutes(1); 
                  policy.QueueLimit = 0;               
                  policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
              });

              options.RejectionStatusCode = 429;
          });
            return services;
        }

    }
}
