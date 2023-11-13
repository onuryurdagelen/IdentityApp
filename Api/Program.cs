using Api;
using Api.Attributes;
using Api.Extensions;
using Api.Services;
using IdentityApp.Data;
using IdentityApp.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    string provider = builder.Configuration.GetValue<string>("DbProvider");
    switch (provider)
    {
        case "Npgsql":
            options.UseNpgsql(builder.Configuration.GetConnectionString(provider), config =>
            {
                config.MigrationsAssembly("PostgreSql");
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            break;
        case "SqlServer":
        default:
            options.UseSqlServer(builder.Configuration.GetConnectionString(provider), config =>
            {
                config.MigrationsAssembly("SqlServer");
            });
            break;
    }
});
builder.Services.AddScoped<ValidationErrorResponseAttribute>();
//builder.Services.AddScoped<CustomExceptionFilterAttribute>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
                                                   .Where(x => x.Value.Errors.Count > 0)
                                                   .SelectMany(x => x.Value.Errors)
                                                   .Select(x => x.ErrorMessage)
                                                   .ToArray();

        var toReturn = new
        {
            Errors = errors
        };
        return new BadRequestObjectResult(toReturn);
    };

});
// be able to inject JWTService class inside our Controllers
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();

// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    // password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    // for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>() // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
    .AddEntityFrameworkStores<AppDbContext>() // providing our context
    .AddSignInManager<SignInManager<User>>() // make use of Signin manager
    .AddUserManager<UserManager<User>>() // make use of UserManager to create users
    .AddDefaultTokenProviders(); // be able to create tokens for email confirmation

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key we have provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            // the issuer singning key based on JWT:Key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            // the issuer which in here is the api project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            // validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            // don't validate audience (angular side)
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SD.SuperAdminPolicy, policy => policy.RequireRole(SD.SuperAdminPolicy));
    options.AddPolicy(SD.AdminPolicy, policy => policy.RequireRole(SD.AdminRole));
    options.AddPolicy(SD.ManagerPolicy, policy => policy.RequireRole(SD.ManagerRole));
    options.AddPolicy(SD.PlayerPolicy, policy => policy.RequireRole(SD.PlayerRole));
    // Add other policies as needed
    //Or
    options.AddPolicy(SD.AdminOrManagerPolicy, policy => policy.RequireRole(SD.AdminRole,SD.ManagerRole));
    //And
    options.AddPolicy(SD.AdminAndManagerPolicy, policy => policy.RequireRole(SD.AdminRole).RequireRole(SD.ManagerRole));
    //All
    options.AddPolicy(SD.AllRolesPolicy, policy => policy.RequireRole(SD.AdminRole).RequireRole(SD.ManagerRole).RequireRole(SD.PlayerRole));

    //email policy
    options.AddPolicy(SD.AdminEmailPolicy, policy => policy.RequireClaim(ClaimTypes.Email, SD.AdminEmailAddress));
    //paulson surname policy
    options.AddPolicy(SD.PaulsonSurnamePolicy, policy => policy.RequireClaim(ClaimTypes.Surname, SD.PaulsonSurname));
    //manager email and paulson surname policy
    options.AddPolicy(SD.ManagerEmailAndPaulsonSurnamePolicy, policy => policy
    .RequireClaim(ClaimTypes.Email, SD.ManagerEmailAddress)
    .RequireClaim(ClaimTypes.Surname,SD.PaulsonSurname));

    options.AddPolicy(SD.VipPolicy, policy => policy.RequireAssertion(context => SD.VIPPolicy(context)));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular_Policy", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        ;
    });
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.ConfigureExceptionHandler<Program>(app.Environment,app.Services.GetRequiredService<ILogger<Program>>());
app.UseHttpsRedirection();
app.UseCors("Angular_Policy");
// adding UseAuthentication into our pipeline and this should come before UseAuthorization
// Authentication verifies the identity of a user or service, and authorization determines their access rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#region ContextSeed
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
    await contextSeedService.InitializeContextAsync();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(ex.Message, "Failed to initialize and seed the database");
    throw;
}
#endregion

app.Run();
