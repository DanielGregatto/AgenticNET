using AgentInfrastructure.Conversations;
using AgentInfrastructure.Orchestration;
using AgentInfrastructure.Plugins;
using CrossCutting.Identity.Models;
using Data.Context;
using Domain.Configs;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Infrastructure;
using Services.Interfaces;
using System;
using Util;
using Util.Interfaces;

namespace IoC
{
    /// <summary>
    /// Central dependency injection configuration for the application.
    /// Registers all services, repositories, validators, and infrastructure components.
    /// </summary>
    public static class DIBootstrapper
    {
        /// <summary>
        /// Registers all application services and infrastructure dependencies.
        /// This method is called from Program.cs during application startup.
        /// </summary>
        /// <param name="services">The service collection to register dependencies into</param>
        /// <param name="configuration">Application configuration for reading settings</param>
        public static void RegisterCustomServices(IServiceCollection services, IConfiguration configuration)
        {
            // ============================================================================
            // CQRS & MEDIATR CONFIGURATION
            // ============================================================================
            // Register MediatR for command/query handling (CQRS pattern)
            var servicesAssembly = typeof(Services.Core.BaseCommandHandler).Assembly;
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(servicesAssembly);
            });

            // Register mediator handler interface for command/query dispatching
            services.AddScoped<IMediatorHandler, MediatorHandler>();

            // Register all FluentValidation validators from Services assembly
            services.AddValidatorsFromAssembly(servicesAssembly);

            // ============================================================================
            // DATABASE CONTEXT & UNIT OF WORK
            // ============================================================================
            // Configure Entity Framework Core (connection string configured in AppDbContext.OnConfiguring)
            services.AddDbContext<AppDbContext>();

            // Register Unit of Work pattern for transaction management
            services.AddScoped<IUnitOfWork, Data.UnitOfWork.UnitOfWork>();

            // ============================================================================
            // CORE INFRASTRUCTURE SERVICES
            // ============================================================================
            // HTTP context accessor for accessing current request context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // AutoMapper for object-to-object mapping
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Current user abstraction for accessing authenticated user info
            services.AddScoped<IUser, AspNetUser>();

            // ============================================================================
            // AUTHENTICATION & SECURITY SERVICES
            // ============================================================================
            // JWT token generation service
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Refresh token service for token renewal
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Cloudflare Turnstile CAPTCHA validation service
            services.AddScoped<ITurnstileValidatorService, TurnstileValidatorService>();

            // ============================================================================
            // EMAIL SERVICES
            // ============================================================================
            // Email service for sending application emails
            services.AddScoped<IEmailService, EmailService>();

            // ASP.NET Core Identity email sender (wraps EmailService)
            services.AddSingleton<IEmailSender<ApplicationUser>, EmailService>();

            // ============================================================================
            // UTILITY SERVICES
            // ============================================================================
            // Base64 string encoding/decoding utilities
            services.AddScoped<IHandlerBase64String, HandlerBase64String>();

            // Browser detection and user agent parsing
            services.AddScoped<IHandlerBrowser, HandlerBrowser>();

            // Image processing and manipulation utilities
            services.AddScoped<IHandlerImage, HandlerImage>();

            // Temporary data storage via cookies
            services.AddScoped<IHandlerTempInfo, HandlerCookie>();

            // Text manipulation utilities
            services.AddScoped<IHandlerText, HandlerText>();

            // Validation helpers (CPF/CNPJ, phone numbers, etc.)
            services.AddScoped<IHandlerValidation, HandlerValidation>();

            // ============================================================================
            // CONFIGURATION OPTIONS
            // ============================================================================
            // Bind configuration sections to strongly-typed configuration classes
            services.Configure<AgentOrchestrationOptions>(
                configuration.GetSection(AgentOrchestrationOptions.SectionName));

            services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
            services.Configure<SecretEndpointConfig>(configuration.GetSection("SecretEndpointConfig"));
            services.Configure<AzureStorageConfig>(configuration.GetSection("AzureStorage"));
            services.Configure<TurnstileConfig>(configuration.GetSection("Turnstile"));
            services.Configure<RateLimitConfig>(configuration.GetSection("RateLimit"));

            // ============================================================================
            // AGENT ORCHESTRATION
            // ============================================================================
            services.AddSingleton<IConversationStore, InMemoryConversationStore>();
            services.AddScoped<IProductCatalogPlugin, ProductCatalogPlugin>();
            services.AddScoped<IAgentOrchestrator, SemanticKernelOrchestrator>();
        }
    }
}
