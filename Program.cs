using apiv4.Data;
using apiv4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

//**********************************************************************
// 1. Hämta Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// 2. Registrera DB Context för DI
builder.Services.AddDbContext<ApiContext>(options =>
    options.UseSqlServer(connectionString));

//**********************************************************************
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
    // Roller
    .AddRoles<IdentityRole>()
    // Identity ska använda Entity Framework Core och ApiContext
    .AddEntityFrameworkStores<ApiContext>()
    // Stöd för t.ex. lösenordsåterställning, e-postbekräftelse)
    .AddDefaultTokenProviders()
    // SignInManager<TUser> behövs för PasswordSignInAsync
    .AddSignInManager();

//***********************************************************************
//Konfiguration av cookies som håller koll på inloggning
builder.Services.ConfigureApplicationCookie(options =>
{
    //För API
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
    //Dev
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
//Kollar hur det går med inlogg 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c=>
{ 
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "apiv4", Version = "v1" }); 
});

// Add services to the container.
builder.Services.AddControllers();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Måste köras FÖRE UseSwaggerUI
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "apiv4 v1");
    });

}
;

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();