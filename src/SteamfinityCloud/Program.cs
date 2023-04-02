using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IRoleInitializer, RoleInitializer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var application = builder.Build();

application.UseHttpsRedirection();
application.MapControllers();

if (application.Environment.IsDevelopment())
{
    _ = application.UseSwagger();
    _ = application.UseSwaggerUI();
}

using var roleInitializerScope = application.Services.CreateAsyncScope();
await roleInitializerScope.ServiceProvider.GetRequiredService<IRoleInitializer>().InitializeRolesAsync();

application.Run();
