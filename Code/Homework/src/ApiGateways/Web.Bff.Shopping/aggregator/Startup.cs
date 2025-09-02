using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Log.Information("Startup constructed. Has configuration: {HasConfig}", Configuration != null);
        }

        public IConfiguration Configuration { get; }

        // ----------------------------
        // ConfigureServices
        // ----------------------------
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("ConfigureServices starting for WebShoppingAgg");

            // Dump common env/config values up front
            Log.Information("ASPNETCORE_ENVIRONMENT = {Env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            Log.Information("ASPNETCORE_URLS        = {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
            Log.Information("PATH_BASE              = {PathBase}", Configuration["PATH_BASE"]);
            Log.Information("IdentityUrlExternal    = {IdentityUrlExternal}", Configuration["IdentityUrlExternal"]);

            // Pull the health check URLs and log them
            var catalogUrlHC  = Configuration["CatalogUrlHC"];
            var orderingUrlHC = Configuration["OrderingUrlHC"];
            var basketUrlHC   = Configuration["BasketUrlHC"];
            var identityUrlHC = Configuration["IdentityUrlHC"];
            var paymentUrlHC  = Configuration["PaymentUrlHC"];

            Log.Information("CatalogUrlHC  = {Url}", catalogUrlHC);
            Log.Information("OrderingUrlHC = {Url}", orderingUrlHC);
            Log.Information("BasketUrlHC   = {Url}", basketUrlHC);
            Log.Information("IdentityUrlHC = {Url}", identityUrlHC);
            Log.Information("PaymentUrlHC  = {Url}", paymentUrlHC);

            // Health checks + per-URL validation with logging
            var hc = services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());

            AddUrlGroupLogged(hc, catalogUrlHC,  "catalogapi-check",  "catalogapi");
            AddUrlGroupLogged(hc, orderingUrlHC, "orderingapi-check", "orderingapi");
            AddUrlGroupLogged(hc, basketUrlHC,   "basketapi-check",   "basketapi");
            AddUrlGroupLogged(hc, identityUrlHC, "identityapi-check", "identityapi");
            AddUrlGroupLogged(hc, paymentUrlHC,  "paymentapi-check",  "paymentapi");

            Log.Information("Registering MVC/Auth/Devspaces/App/Grpc services...");
            services.AddCustomMvc(Configuration)
                    .AddCustomAuthentication(Configuration)
                    .AddDevspaces()
                    .AddApplicationServices()
                    .AddGrpcServices();

            Log.Information("ConfigureServices completed");
        }

        // Helper that validates & logs before adding the URL group
        private static void AddUrlGroupLogged(IHealthChecksBuilder builder, string url, string name, string tag)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error("{Name}: configuration value is null/empty; skipping health check.", name);
                return;
            }

            try
            {
                var uri = new Uri(url, UriKind.Absolute);
                Log.Information("Adding health check {Name} -> {Uri}", name, uri);
                builder.AddUrlGroup(uri, name: name, tags: new[] { tag });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Name}: invalid URI value '{Url}'; skipping health check.", name, url);
            }
        }

        // ----------------------------
        // Configure
        // ----------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var log = loggerFactory.CreateLogger<Startup>();
            log.LogInformation("Configure starting. Environment={Env} ContentRoot={Root}",
                env.EnvironmentName, env.ContentRootPath);

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                log.LogInformation("Using PATH_BASE: {PathBase}", pathBase);
                app.UsePathBase(pathBase);
            }
            else
            {
                log.LogInformation("No PATH_BASE configured.");
            }

            if (env.IsDevelopment())
            {
                log.LogInformation("Environment is Development: enabling DeveloperExceptionPage");
                app.UseDeveloperExceptionPage();
            }

            log.LogInformation("Enabling HTTPS redirection");
            app.UseHttpsRedirection();

            // Swagger + UI
            try
            {
                var swaggerBase = !string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty;
                var swaggerEndpoint = $"{swaggerBase}/swagger/v1/swagger.json";
                log.LogInformation("Configuring Swagger. Endpoint={SwaggerEndpoint}", swaggerEndpoint);

                var identityExternal = Configuration.GetValue<string>("IdentityUrlExternal");
                log.LogInformation("Swagger OAuth IdentityUrlExternal={IdentityUrlExternal}", identityExternal);

                app.UseSwagger()
                   .UseSwaggerUI(c =>
                   {
                       c.SwaggerEndpoint(swaggerEndpoint, "Purchase BFF V1");
                       c.OAuthClientId("webshoppingaggswaggerui");
                       c.OAuthClientSecret(string.Empty);
                       c.OAuthRealm(string.Empty);
                       c.OAuthAppName("web shopping bff Swagger UI");
                   });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed configuring Swagger");
                throw;
            }

            log.LogInformation("Adding routing, CORS, authentication, authorization");
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            log.LogInformation("Mapping endpoints: controllers + /hc + /liveness");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });

            log.LogInformation("Configure completed");
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring authentication (JWT Bearer)");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityUrl = configuration.GetValue<string>("urls:identity");
            Log.Information("Identity Authority (urls:identity) = {IdentityUrl}", identityUrl);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "webshoppingagg";
                Log.Information("JwtBearer configured: Authority={Authority} Audience={Audience} HttpsMetadata={Https}",
                    options.Authority, options.Audience, options.RequireHttpsMetadata);
            });

            return services;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring MVC + options + Swagger");

            services.AddOptions();
            services.Configure<UrlsConfig>(configuration.GetSection("urls"));
            LogUrlsSection(configuration);

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    Log.Information("System.Text.Json configured: WriteIndented={Indented}", options.JsonSerializerOptions.WriteIndented);
                });

            services.AddSwaggerGen(options =>
            {
                Log.Information("Configuring SwaggerGen");

                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Shopping Aggregator for Web Clients",
                    Version = "v1",
                    Description = "Shopping Aggregator for Web Clients"
                });

                var idExternal = configuration.GetValue<string>("IdentityUrlExternal");
                Log.Information("Swagger OAuth uses IdentityUrlExternal={IdentityUrlExternal}", idExternal);

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{idExternal}/connect/authorize"),
                            TokenUrl = new Uri($"{idExternal}/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "webshoppingagg", "Shopping Aggregator for Web Clients" }
                            }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options =>
            {
                Log.Information("Adding CORS policy 'CorsPolicy' (AllowAnyHeader/Method, credentials allowed)");
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            Log.Information("AddCustomMvc completed");
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            Log.Information("Registering application services, handlers, HttpClient(s)");

            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHttpClient<IOrderApiClient, OrderApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddDevspacesSupport();

            Log.Information("HttpClient IOrderApiClient registered with authorization delegating handler");
            return services;
        }

        public static IServiceCollection AddGrpcServices(this IServiceCollection services)
        {
            Log.Information("Registering gRPC services/clients");
            services.AddTransient<GrpcExceptionInterceptor>();

            services.AddScoped<IBasketService, BasketService>();
            services.AddGrpcClient<Basket.BasketClient>((sp, options) =>
            {
                var urls = sp.GetRequiredService<IOptions<UrlsConfig>>().Value;
                Log.Information("Configuring gRPC BasketClient -> {Address}", urls.GrpcBasket);
                options.Address = new Uri(urls.GrpcBasket);
            }).AddInterceptor<GrpcExceptionInterceptor>();

            services.AddScoped<ICatalogService, CatalogService>();
            services.AddGrpcClient<Catalog.CatalogClient>((sp, options) =>
            {
                var urls = sp.GetRequiredService<IOptions<UrlsConfig>>().Value;
                Log.Information("Configuring gRPC CatalogClient -> {Address}", urls.GrpcCatalog);
                options.Address = new Uri(urls.GrpcCatalog);
            }).AddInterceptor<GrpcExceptionInterceptor>();

            services.AddScoped<IOrderingService, OrderingService>();
            services.AddGrpcClient<OrderingGrpc.OrderingGrpcClient>((sp, options) =>
            {
                var urls = sp.GetRequiredService<IOptions<UrlsConfig>>().Value;
                Log.Information("Configuring gRPC OrderingGrpcClient -> {Address}", urls.GrpcOrdering);
                options.Address = new Uri(urls.GrpcOrdering);
            }).AddInterceptor<GrpcExceptionInterceptor>();

            Log.Information("AddGrpcServices completed");
            return services;
        }

        // Helper: log the "urls" section contents
        private static void LogUrlsSection(IConfiguration configuration)
        {
            var u = configuration.GetSection("urls");
            Log.Information("urls:basket     = {Val}", u["basket"]);
            Log.Information("urls:catalog    = {Val}", u["catalog"]);
            Log.Information("urls:orders     = {Val}", u["orders"]);
            Log.Information("urls:identity   = {Val}", u["identity"]);
            Log.Information("urls:grpcBasket = {Val}", u["grpcBasket"]);
            Log.Information("urls:grpcCatalog= {Val}", u["grpcCatalog"]);
            Log.Information("urls:grpcOrdering= {Val}", u["grpcOrdering"]);
        }
    }


}
