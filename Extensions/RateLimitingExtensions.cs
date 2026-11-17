using System.Globalization;
using System.Threading.RateLimiting;
using ClinicManagementSystem.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagementSystem.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string BookingPolicy = "booking";

    public static IServiceCollection AddClinicRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.SectionName)
                                    .Get<RateLimitSettings>() ?? new RateLimitSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // 1) Safety net for EVERY request (fixed window)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.Global.WindowSeconds),
                        QueueLimit = 0
                    }));

            // 2) Strict named policies, opted into with [EnableRateLimiting("...")]
            options.AddPolicy(AuthPolicy, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.Auth.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.Auth.WindowSeconds),
                        QueueLimit = 0
                    }));

            options.AddPolicy(BookingPolicy, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.Booking.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.Booking.WindowSeconds),
                        QueueLimit = 0
                    }));

            // 3) What the client sees when rejected
            options.OnRejected = async (context, ct) =>
            {
                var response = context.HttpContext.Response;
                int? retrySeconds = null;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retrySeconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
                    response.Headers.RetryAfter =
                        retrySeconds.Value.ToString(CultureInfo.InvariantCulture);
                }

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = "You are sending requests too quickly. Please wait and try again."
                };

                if (retrySeconds is { } seconds)
                    problem.Extensions["retryAfterSeconds"] = seconds;

                response.StatusCode = StatusCodes.Status429TooManyRequests;
                await response.WriteAsJsonAsync(problem, ct);
            };
        });

        return services;
    }

    // The partition key: one bucket per client IP.
    private static string ClientKey(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}