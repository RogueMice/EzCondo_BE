using EzCondo_Data.Domain;
using EzCondo_Data.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service.IService;
using Service.Service;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Security.Claims;
using EzConDo_Service.Interface;
using EzConDo_Service.Implement;
using EzConDo_Service.Cloudinary;
using EzConDo_Service.CloudinaryIntegration;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()  
            .AllowAnyHeader()   
            .AllowAnyMethod();  
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// config Swagger that using auth on it ...
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Authorization header using the bearer Scheme",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

//Add Jwt setting
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            // error 401
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var json = JsonConvert.SerializeObject(new
                {
                    status = 401,
                    message = "You need to login to use this resource!"
                });
                return context.Response.WriteAsync(json);
            },
            // error 403
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var json = JsonConvert.SerializeObject(new
                {
                    status = 403,
                    message = "You can't access this resource!"
                });
                return context.Response.WriteAsync(json);
            }
        };
    });

//Authorize using Role
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role.ToString().ToLower(), "admin"));
});

//Configure Connection String
builder.Services.AddDbContext<EzCondo_Data.Context.ApartmentDbContext>(options =>
                                                        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICitizenService, CitizenService>();
builder.Services.AddScoped<IService_service,ServiceOfSerivce>();
builder.Services.AddScoped<IService_ImageService, ServiceImageOfService>();


//Add cloud service
builder.Services.AddScoped<CloudinaryService>();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

var app = builder.Build();

//use cors
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Declare Authen
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
