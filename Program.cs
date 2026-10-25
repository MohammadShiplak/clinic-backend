using ClinicManagementSystem.API;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using ClinicManagementSystem.API.Services;
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

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));


builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorAvailabilityService, DoctorAvailabilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationInboxService, NotificationInboxService>();

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