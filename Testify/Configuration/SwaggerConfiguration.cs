using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Testify.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);

            // Include XML comments generated from C# doc comments on controllers/actions.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' followed by your JWT token. Example: 'Bearer eyJhbGc...'"
            });
            
            options.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
            {
                Name = ".AspNetCore.Identity.Application",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Description = "Cookie authentication using ASP.NET Core Identity"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Cookie"
                        }
                    },
                    new string[] {}
                }
            });

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Testify API",
                Version = "v1",
                Description = "API documentation for Testify project management and testing platform"
            });
        });

        return services;
    }
}
