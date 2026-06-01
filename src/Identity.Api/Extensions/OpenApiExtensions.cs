using Microsoft.OpenApi;

namespace Identity.Api.Extensions
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApiWithSecurity(
    this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, ct) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token"
                    });
                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
