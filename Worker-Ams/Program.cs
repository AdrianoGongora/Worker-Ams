using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Worker_Ams.Database;
using Worker_Ams.Endpoints;
using Worker_Ams.Exntensions;
using Worker_Ams.Repositories.Datos;
using Worker_Ams.Repositories.Motors;
using Worker_Ams.Repositories.Users;
using Worker_Ams.Services.Jwt;
using Worker_Ams.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);
const string cors = "Cors";

string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: cors,
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.WithOrigins("*");
            corsPolicyBuilder.AllowAnyMethod();
            corsPolicyBuilder.AllowAnyHeader();
        });
});

builder.Services.AddDbContext<ApplicationDbContext>((options) =>
{
    options.UseNpgsql(databaseConnectionString);
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMotorRepository, MotorRepository>();
builder.Services.AddScoped<IDatosRepository, DatosRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapUserEndpoints();
app.MapMotorEndpoints();
app.UseHttpsRedirection();

app.ApplyMigrations();

app.Run();

public partial class Program;