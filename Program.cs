using ClinicManagementSystem.API;
using ClinicManagementSystem.API.Auth;
using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using ClinicManagementSystem.API.Services;
using ClinicManagementSystem.API.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Bootstrap logger: only alive until the real logger is configured below.
// It catches crashes that happen DURING startup (bad config, invalid options).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace the default logging with Serilog, configured from appsettings.json.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ---- Services ----
    builder.Services.AddDbContext<ClinicDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicDb")));

    builder.Services.AddOptions<AppointmentSettings>()
        .Bind(builder.Configuration.GetSection(AppointmentSettings.SectionName))
        .Validate(s => s.SlotMinute > 0 && s.WorkdayStart < s.WorkdayEnd,
                  "Appointments settings are invalid.")
        .ValidateOnStart();

    builder.Services.AddOptions<ReminderSettings>()
        .Bind(builder.Configuration.GetSection(ReminderSettings.SectionName))
        .Validate(s => s.LeadTimeHours > 0 && s.CheckIntervalMinutes > 0, "Reminder settings are invalid.")
        .ValidateOnStart();

    builder.Services.AddOptions<EmailSettings>()
        .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
        .Validate(s => s.DispatchIntervalSeconds > 0 && s.MaxAttempts > 0
                       && (!s.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
                           || (!string.IsNullOrWhiteSpace(s.Host) && !string.IsNullOrWhiteSpace(s.FromAddress))),
                  "Email settings are invalid.")
        .ValidateOnStart();

    builder.Services.AddOptions<JwtSettings>()
        .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
        .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer)
                       && !string.IsNullOrWhiteSpace(s.Audience)
                       && s.SigningKey.Length >= 32
                       && s.AccessTokenMinutes is >= 1 and <= 1440,
                  "Jwt settings are invalid (signing key must be at least 32 characters).")
        .ValidateOnStart();

    // ---- Authentication: "who are you?" ----
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();

    builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .Configure<IOptions<JwtSettings>>((bearer, jwt) =>
        {
            var s = jwt.Value;

            bearer.MapInboundClaims = false;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = s.Issuer,
                ValidateAudience = true,
                ValidAudience = s.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RoleClaimType = AppClaims.Role
            };
        });

    // ---- Authorization: "what may you do?" ----
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddSingleton<IAuthorizationHandler, DoctorSelfOrAdminHandler>();

    builder.Services.AddAuthorization(options =>
    {
        // Secure by default: every endpoint needs a token unless it says [AllowAnonymous].
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        options.AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin));
        options.AddPolicy(Policies.StaffOnly, p => p.RequireRole(Roles.Admin, Roles.Doctor));
        options.AddPolicy(Policies.PatientOnly, p => p.RequireRole(Roles.Patient));
        options.AddPolicy(Policies.DoctorSelfOrAdmin, p => p.AddRequirements(new DoctorSelfOrAdminRequirement()));
    });

    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

    // ---- Error handling ----
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSingleton<IPasswordService, PasswordService>();

    if (string.Equals(builder.Configuration["Email:Provider"], "Smtp", StringComparison.OrdinalIgnoreCase))
        builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
    else
        builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();

    builder.Services.AddScoped<IDoctorService, DoctorService>();
    builder.Services.AddScoped<IAppointmentService, AppointmentService>();
    builder.Services.AddScoped<IDoctorAvailabilityService, DoctorAvailabilityService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<INotificationInboxService, NotificationInboxService>();
    builder.Services.AddScoped<IAppointmentReminderService, AppointmentReminderService>();
    builder.Services.AddHostedService<AppointmentReminderWorker>();
    builder.Services.AddScoped<INotificationEmailDispatcher, NotificationEmailDispatcher>();
    builder.Services.AddHostedService<NotificationEmailWorker>();
    builder.Services.AddScoped<IPatientService, PatientService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the access token only (no 'Bearer ' prefix)."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ---- Pipeline (order matters) ----
    app.UseSerilogRequestLogging();   // FIRST: wraps everything, logs one line per request
    app.UseExceptionHandler();        // converts unhandled exceptions into a clean 500

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)   // EF tooling aborts the host on purpose
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();   // flush buffered file writes before the process exits
}