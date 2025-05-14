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
using EzConDo_Service.SignalR_Integration;
using EzCondo_Data.Context;
using EzConDo_Service.PayOsIntergration;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://0.0.0.0:7254", "https://ez-condo.vercel.app") // Specify allowed origins
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Allow credentials
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
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/notificationHub")) // <-- url hub need correct
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            //Validate TokenVersion 
            OnTokenValidated = async context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenVersionFromToken = claimsIdentity?.FindFirst("TokenVersion")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenVersionFromToken))
                {
                    context.Fail("Token is invalid or malformed");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApartmentDbContext>();
                var user = await dbContext.Users.FindAsync(Guid.Parse(userId));

                if (user == null || user.TokenVersion.ToString() != tokenVersionFromToken)
                {
                    context.Fail("TokenVersion mismatch");
                }
            },
            //error 401
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var failureMessage = context.AuthenticateFailure?.Message;

                var message = failureMessage != null && failureMessage.Contains("TokenVersion mismatch")
                                                ? "Token is invalid or expired!"
                                                : "You need to login to use this resource!";

                var json = JsonConvert.SerializeObject(new
                {
                    status = 401,
                    message = message
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
    options.AddPolicy("ManagerOrResident", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type.ToLower() == ClaimTypes.Role.ToLower() &&
                (c.Value.ToLower() == "resident" || c.Value.ToLower() == "manager")));
    });
});

//Configure Connection String
builder.Services.AddDbContext<EzCondo_Data.Context.ApartmentDbContext>(options =>
                                                        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//Config Hangfire use SQL Server
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"), new Hangfire.SqlServer.SqlServerStorageOptions
          {
              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
              QueuePollInterval = TimeSpan.FromSeconds(15),
              UseRecommendedIsolationLevel = true,
              DisableGlobalLocks = true
          }));

// Add Hangfire background
builder.Services.AddHangfireServer();


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
builder.Services.AddScoped<IElectricService, ElectricService>();
builder.Services.AddScoped<IWaterService, WaterService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IParkingLotService, ParkingLotService>();

//Add cloud service
builder.Services.AddScoped<CloudinaryService>();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));


// Add MemoryCache
builder.Services.AddMemoryCache();

//Add SignalR
builder.Services.AddSignalR();

// Register NotificationHub
builder.Services.AddSingleton<NotificationHub>();

// Create Firebase
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("D:\\Capstone2\\Firebase(Key)\\ezcondo-73fc4-firebase-adminsdk-fbsvc-e80056737f.json")
});

//Add PayOS
builder.Services.Configure<PayQrSettings>(
    builder.Configuration.GetSection("PayOsSettings"));

builder.Services.Configure<PayOsClientSettings>(
    builder.Configuration.GetSection("PayOS"));

var app = builder.Build();

app.UseRouting();

//use cors
app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Declare Authen
app.UseAuthentication();
app.UseAuthorization();

//Use Hub

app.MapHub<NotificationHub>("/notificationHub");


app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

// Use Hangfire dashboard
app.UseHangfireDashboard();

//Các job tự động 
//Water 
RecurringJob.AddOrUpdate<IWaterService>(
    "water-bills-overdue-job",
    svc => svc.UpdateOverdueWaterBillsAsync(),
    Cron.Daily(hour: 0, minute: 0),
    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
);

//Electric
RecurringJob.AddOrUpdate<IElectricService>(
    "electric-bills-overdue-job",
    svc => svc.UpdateOverdueElectricBillsAsync(),
    Cron.Daily(hour: 0, minute: 0),
    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
);

//Parking Lot
RecurringJob.AddOrUpdate<IParkingLotService>(
    "parking-bills-overdue-job",
    svc => svc.UpdateOverdueParkingBillsAsync(),
    Cron.Daily(hour: 0, minute: 0),
    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
);

RecurringJob.AddOrUpdate<IParkingLotService>(
    "parking-bills-overdue-job",
    svc => svc.UpdateOverdueParkingBillsAsync(),
    Cron.Daily(hour: 0, minute: 0),
    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
);

app.Run();
