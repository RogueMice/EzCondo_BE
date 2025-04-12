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
using EzCondo_API.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using EzConDo_Service.FirebaseIntegration;
using EzCondo_Data.Domain;

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
    options.AddPolicy("Manager", policy =>
        policy.RequireClaim(ClaimTypes.Role.ToString().ToLower(), "manager"));
    options.AddPolicy("Resident", policy =>
        policy.RequireClaim(ClaimTypes.Role.ToString().ToLower(), "resident"));
    options.AddPolicy("AdminOrManager", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type.ToLower() == ClaimTypes.Role.ToLower() &&
                (c.Value.ToLower() == "admin" || c.Value.ToLower() == "manager")));
    });
});

//Configure Connection String
builder.Services.AddDbContext<EzCondo_Data.Context.ApartmentDbContext>(options =>
                                                        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICitizenService, CitizenService>();
builder.Services.AddScoped<IService_service,ServiceOfSerivce>();
builder.Services.AddScoped<IService_ImageService, ServiceImageOfService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserDeviceService, UserDeviceService>();
builder.Services.AddScoped<IFirebasePushNotificationService, FirebasePushNotificationService>();
builder.Services.AddScoped<IPrice_electric_service, PriceElectricTierService>();
builder.Services.AddScoped<IPriceWaterTierService, PriceWaterTierService>();
builder.Services.AddScoped<IPriceParkingLotService, PriceParkingLotService>();
builder.Services.AddScoped<IHouseHoldMemberService, HouseHoldMemberService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<INotificationImageService, NotificationImageService>();
builder.Services.AddScoped<I_incidentService, IncidentService>();
builder.Services.AddScoped<I_IncidentImage, IncidentImageService>();
builder.Services.AddScoped<IElectricMeterService, ElectricMetterService>();

//Add cloud service
builder.Services.AddScoped<CloudinaryService>();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));


// Add MemoryCache
builder.Services.AddMemoryCache();

//Add SignalR
builder.Services.AddSignalR();

// Create Firebase
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("D:\\Capstone2\\Firebase(Key)\\ezcondo-73fc4-firebase-adminsdk-fbsvc-e80056737f.json")
});

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


app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();
