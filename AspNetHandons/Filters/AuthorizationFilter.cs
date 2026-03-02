using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace AspNetHandons.Filters
{
    public class AuthorizationFilter : IAsyncActionFilter
    {
        private readonly string API_KEY = "API-KEY";
        private readonly IConfiguration _configuration;

        public AuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task OnActionExecutionAsync(
         ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Do something before the action executes.
            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY, out var extractedapiKey))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    statusCode = 401,
                    message = "Missing API key"
                });
                return;
            }

            var apiKey = _configuration.GetValue<string>("ApiKey");
            if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedapiKey))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    statusCode = 401,
                    message = "Invalid API Key"
                });
                return;
            }

            await next();
        }
    }
}
