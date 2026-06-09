using Microsoft.OpenApi.Models;
using System.Reflection;

namespace UI.API.Configurations
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "AgenticNET API",
                        Version = "v1",
                        Description =
                            "AI platform built on .NET 8 and Azure, AgenticNET is an open-source, Azure-native platform for building, deploying and operating enterprise AI applications with .NET. It combines intelligent routing, RAG, reviewer loops, infrastructure automation and full decision traceability into a single production-ready platform.\r\n" +
                            "Messages sent to `POST /api/v1/chat` are classified by a **RouterAgent** and dispatched " +
                            "to the best specialist agent (GeneralAdvisor, ProductCatalog, SupplierAdvisor, ...). " +
                            "Agents can call plugins (RAG over Azure AI Search, product catalog queries) and " +
                            "optionally run through a **ReviewerAgent** confidence loop before the answer is returned.\n\n" +
                            "Every response includes a `trace` array that records which agent was selected, which " +
                            "plugins were called, and what the reviewer scored — giving full decision traceability.\n\n" +
                            "**Authentication:** all endpoints except `/api/v1/auth/*` require " +
                            "`Authorization: Bearer <jwt>`. Obtain a token via `POST /api/v1/auth/login`."
                    });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "bearer",
                            Name = "Authorization",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key used for authentication on specific endpoints. Example: apikey: {your-key}",
                    Name = "apikey",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKeyScheme"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Add Accept-Language header parameter to all endpoints
                c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
            });
        }
    }
}
