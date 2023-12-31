using System.Text;
using Asp.Versioning;
using Health_Tracker.Authentication.Configuration;
using HealthTracker.DataService.Data;
using HealthTracker.DataService.IConfiguration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add BbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

// Configure UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure the API versioning
builder.Services.AddApiVersioning(options =>
{
    // Provide to the client the different Api versions that we have
    options.ReportApiVersions = true;

    // This will allow the api to automatically provide a default version
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

// Getting the secret from the config
var _JwtSecret = builder.Configuration.GetValue<string>("JwtConfig:Secret");

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSecret)),
    ValidateIssuer = false, // ToDo Update
    ValidateAudience = false, // ToDo Update
    RequireExpirationTime = false, // ToDo Update
    ValidateLifetime = true
};

// Injecting into our DI Container
builder.Services.AddSingleton(tokenValidationParameters);


builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.RequireHttpsMetadata = true;
    jwt.TokenValidationParameters = tokenValidationParameters;
});

//builder.Services.AddDefaultIdentity<IdentityUser>(
//    options => options.SignIn.RequireConfirmedAccount = true)
//.AddEntityFrameworkStores<AppDbContext>();

// This is to include Role functionality
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = true)
.AddEntityFrameworkStores<AppDbContext>();

// Update the JWT config from the settings
var _JwtConfig = builder.Configuration.GetSection("JwtConfig");
builder.Services.Configure<JwtConfig>(_JwtConfig);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DepartmentPolicy",
         policy => policy.RequireClaim("Department"));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Builds the web application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() | app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
