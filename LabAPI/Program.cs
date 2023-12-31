using DbOperations_ADO;
using Entities.Models;
using ExternalEntities;
using ExternalEntities.Misc;
using LabAPI;
using LabAPI.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Repository.Interfaces;
using System.Runtime;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Configure the DbSettings using the Options Pattern
builder.Services.Configure<DBSettings>(builder.Configuration.GetSection("ConnectionStrings"));


builder.Services.AddDbContext<LabContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"));
});

//For Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = false;
    
})      .AddEntityFrameworkStores<LabContext>()
        .AddDefaultTokenProviders();


builder.Services.AddTransient<ConnectionManagement.ConnectionManager>();
builder.Services.AddTransient<IToDoRepository, DbOperations_ADO.DbOperations>();

//builder.Services.AddScoped<IUserStore<IdentityUser>, IdentityUserRepository_ADO>();
//builder.Services.AddScoped<IRoleStore<IdentityRole>, IdentityRoleRepository_ADO>();

//For JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtConfig"));

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt => {
    var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

    jwt.SaveToken = true;
    jwt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, 
        IssuerSigningKey = new SymmetricSecurityKey(key), 
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "Role",
        NameClaimType = "Id",
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ResourceOwnershipPolicy", policy =>
        policy.Requirements.Add(new ResourceOwnershipRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, OwnershipAuthorizationHandler>();

// If using Kestrel:
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// If using IIS:
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<Ex_ToDo>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<List<Ex_ToDo>>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<Boolean>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<string>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<EX_Login>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<EX_TokenResult>());
    options.JsonSerializerOptions.Converters.Add(new ResponseObjectConverter<EX_UserRegister>()); 
}); 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    var roles = new[] { "User", "Admin", "Guest" };

//    foreach (var role in roles)
//    {
//        if(!await roleManager.RoleExistsAsync(role))
//        { 
//            await roleManager.CreateAsync(new IdentityRole(role)); 
//        }
//    }
//}

    app.Run();
