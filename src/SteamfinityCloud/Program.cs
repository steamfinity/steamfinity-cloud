using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Steamfinity.Cloud;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IRoleInitializer, RoleInitializer>();
builder.Services.AddScoped<ILibraryManager, LibraryManager>();
builder.Services.AddScoped<IMembershipManager, MembershipManager>();
builder.Services.AddScoped<IPermissionManager, PermissionManager>();
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<ILimitProvider, LimitProvider>();

builder.Services.AddScoped<ISteamApi, SteamApi>();
builder.Services.AddHttpClient<ISteamApi, SteamApi>(client => client.BaseAddress = new Uri("https://api.steampowered.com"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:Schemes:Bearer:Issuer"];
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Authentication:Schemes:Bearer:Audience"];

    var issuerSigningKey = builder.Configuration["Authentication:Schemes:Bearer:IssuerSigningKey"]
    ?? throw new ConfigurationMissingException("Authentication:Schemes:Bearer:IssuerSigningKey");

    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.Users, policy => policy.RequireRole(RoleNames.User, RoleNames.Administrator));
    options.AddPolicy(PolicyNames.Administrators, policy => policy.RequireRole(RoleNames.Administrator));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        Name = "Bearer",
        Description = "Enter the JWT bearer authentication token.",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,

        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var application = builder.Build();

application.UseHttpsRedirection();
application.UseAuthentication();
application.UseAuthorization();
application.MapControllers();

if (application.Environment.IsDevelopment())
{
    _ = application.UseSwagger();
    _ = application.UseSwaggerUI();
}

using var roleInitializerScope = application.Services.CreateAsyncScope();
await roleInitializerScope.ServiceProvider.GetRequiredService<IRoleInitializer>().InitializeRolesAsync();

application.Run();
