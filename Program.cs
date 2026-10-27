using ClinicManagementSystem.API;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using ClinicManagementSystem.API.Services;
using ClinicManagementSystem.API.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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


builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));


builder.Services.AddSingleton(TimeProvider.System);

if (string.Equals(builder.Configuration["Email:Provider"], "Smtp", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();


builder.Services.AddControllers();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---- Pipeline (order matters) ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();          // serves wwwroot, before CORS/auth
app.UseCors("Frontend");       // before authentication/authorization
app.UseAuthorization();
app.MapControllers();

app.Run();