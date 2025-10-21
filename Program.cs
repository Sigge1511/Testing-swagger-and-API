using apiv4.Data;
using apiv4.Models;
using apiv4.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

//***************** DATABASFIX *****************************************************
// 1. Hämta Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// 2. Registrera DB Context för DI
builder.Services.AddDbContext<ApiContext>(options =>
    options.UseSqlServer(connectionString));

//**************** IDENTITY + EF CORE ******************************************************
// Identity Setup
// Använd AddIdentityCore<TUser> + AddSignInManager()) osv.
builder.Services.AddIdentityCore<ApiUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // (valfritt) lösenkrav i dev - håll dessa borta från din production-appsettings!
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Lägg till andra krav här, t.ex. Lockout-inställningar
    // options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddRoles<IdentityRole>()
    // Identity ska använda Entity Framework Core och ApiContext
    .AddEntityFrameworkStores<ApiContext>()
    // Stöd för t.ex. lösenordsåterställning
    .AddDefaultTokenProviders()
    .AddSignInManager();


//Cors skyddar vad vi delar mellan olika domäner
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

//**************** AUTHENTICATION ******************************************************

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
    };
});














//KOMMENTERAR UT ALLT OM IDENTITY OCH COOKIES 
////***********************************************************************
////Konfiguration av cookies som håller koll på inloggning
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    //För API
//    options.Events.OnRedirectToLogin = context =>
//    {
//        context.Response.StatusCode = 401;
//        return Task.CompletedTask;
//    };
//    options.Events.OnRedirectToAccessDenied = context =>
//    {
//        context.Response.StatusCode = 403;
//        return Task.CompletedTask;
//    };
//    //Dev
//    options.Cookie.SameSite = SameSiteMode.None;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//});
////Kollar hur det går med inlogg 
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
//    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
//})
//.AddIdentityCookies();






// Add services to the container.
builder.Services.AddControllers();

// Lägg till denna rad för att repo ska fungera
builder.Services.AddScoped<IBookRepo,BookRepo>();


//************* SWAGGER OCH CORS *****************************
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{ 
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "apiv4", Version = "v1" }); 
});



//********** BYGG OCH STARTA APPEN ***************************
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // För att undvika att krascha appen pågrund av ett problem med seeding.
    try
    {
        // app.Services ger oss tillgång till IServiceProvider
        await apiv4.SeedData.DataSeeder.SeedAsync(app.Services);

        //// Här kan du lägga till en logger om du vill se att det fungerade
        //var logger = app.Services.GetRequiredService<ILogger<Program>>();
        //logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        // Logga eventuella fel under seeding
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ett fel inträffade under seeding av databasen.");
    }

    app.UseSwagger(); // Måste köras FÖRE UseSwaggerUI
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "apiv4 v1");
    });

};

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    // Fortsätt till nästa middleware (MapControllers)
    await next();

    // Kontrollera om statuskoden fortfarande är 404 (vilket indikerar att resursen inte kunde hittas ELLER att åtkomst nekades)
    if (context.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        // 1. Om användaren INTE är autentiserad men försökte komma åt en [Authorize] resurs:
        if (!context.User.Identity.IsAuthenticated)
        {
            // Tvinga systemet att försöka svara med en 401-utmaning för ditt schema
            // Detta ska få Identity-eventen att köra och skicka 401 istället för 302/404
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        // 2. Om användaren ÄR autentiserad men saknar behörighet/roll (då ska den vara 403, men fås som 404)
        // Vi kan här istället välja att inte ändra något om den är autentiserad, 
        // då en 404 då betyder att resursen faktiskt inte finns.
    }
});
app.MapControllers();

app.Run();