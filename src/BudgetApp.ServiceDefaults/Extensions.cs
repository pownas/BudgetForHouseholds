using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults

/// <summary>
/// Provides extension methods for configuring common application services and behaviors in .NET applications.
/// </summary>
/// <remarks>This static class includes methods to simplify the setup of common infrastructure components such as:
/// <list type="bullet"> <item><description>Service discovery for resolving service endpoints.</description></item>
/// <item><description>Resilience and fault-tolerant HTTP client configurations.</description></item>
/// <item><description>Health checks for monitoring application readiness and liveness.</description></item>
/// <item><description>OpenTelemetry for distributed tracing, metrics, and logging.</description></item> </list> These
/// methods are designed to streamline the configuration of application defaults, enabling developers to focus on
/// business logic while ensuring consistent and reliable service behavior.</remarks>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// Configures the specified builder with a set of default services and behaviors commonly used in applications.
    /// </summary>
    /// <remarks>This method applies the following default configurations to the builder: <list type="bullet">
    /// <item><description>Configures OpenTelemetry for distributed tracing and metrics.</description></item>
    /// <item><description>Adds default health checks to monitor application health.</description></item>
    /// <item><description>Enables service discovery for resolving service endpoints.</description></item>
    /// <item><description>Configures default HTTP client settings, including resilience and service discovery
    /// handlers.</description></item> </list> These defaults are intended to simplify the setup of common application
    /// infrastructure.  Additional customizations can be applied after calling this method.</remarks>
    /// <typeparam name="TBuilder">The type of the builder, which must implement <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">The application builder to configure. Cannot be <see langword="null"/>.</param>
    /// <returns>The configured <typeparamref name="TBuilder"/> instance, enabling further chaining of configuration methods.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry for the specified application builder, enabling logging, metrics, and tracing.
    /// </summary>
    /// <remarks>This method sets up OpenTelemetry logging with formatted messages and scopes, adds
    /// instrumentation for  ASP.NET Core, HTTP clients, and runtime metrics, and configures tracing with
    /// application-specific sources  and instrumentation. Health check and aliveness endpoint requests are excluded
    /// from tracing by default.</remarks>
    /// <typeparam name="TBuilder">The type of the application builder, which must implement <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">The application builder to configure OpenTelemetry for.</param>
    /// <returns>The configured application builder.</returns>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry exporters for the application based on the provided configuration.
    /// </summary>
    /// <remarks>This method checks the application's configuration for specific environment variables or
    /// settings  to determine which OpenTelemetry exporters to enable. For example:  <list type="bullet">  <item> 
    /// <description>If the <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> configuration value is set, the OpenTelemetry Protocol
    /// (OTLP) exporter is enabled.</description>  </item>  <item>  <description>Additional exporters, such as the Azure
    /// Monitor exporter, can be enabled by uncommenting the relevant code and ensuring the required configuration
    /// values are set.</description>  </item>  </list></remarks>
    /// <typeparam name="TBuilder">The type of the application builder, which must implement <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">The application builder used to configure services and middleware.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance, allowing for method chaining.</returns>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    /// <summary>
    /// Adds default health checks to the application, including a liveness check to ensure the application is
    /// responsive.
    /// </summary>
    /// <remarks>This method registers a default liveness health check with the name "self" and the tag
    /// "live". The liveness check always returns a healthy status, indicating that the application is
    /// responsive.</remarks>
    /// <typeparam name="TBuilder">The type of the application builder, which must implement <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">The application builder to which the health checks will be added. Cannot be <see langword="null"/>.</param>
    /// <returns>The application builder instance, allowing further configuration.</returns>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Configures the application to map default health check endpoints for readiness and liveness checks in
    /// development environments.
    /// </summary>
    /// <remarks>This method maps two health check endpoints when the application is running in a development
    /// environment: <list type="bullet"> <item> <description><c>/health</c>: All health checks must pass for the
    /// application to be considered ready to accept traffic.</description> </item> <item>
    /// <description><c>/health/live</c>: Only health checks tagged with the "live" tag must pass for the application to
    /// be considered alive.</description> </item> </list> These endpoints are not mapped in non-development
    /// environments due to potential security implications. For more information, see <see
    /// href="https://aka.ms/dotnet/aspire/healthchecks">Health Checks in ASP.NET Core</see>.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
