using apiv4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c=>
{ 
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "apiv4", Version = "v1" }); 
});

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

builder.Services.AddAuthentication(options=>
{
    options.DefaultAuthenticateScheme=IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme=IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options=>
{
    //För API
    options.Events.OnRedirectToLogin=context=>
    {
        context.Response.StatusCode=401;
        return System.Threading.Tasks.Task.CompletedTask;       
    };
    options.Events.OnRedirectToAccessDenied=context=>
    {
        context.Response.StatusCode=403;
        return System.Threading.Tasks.Task.CompletedTask;       
    };
    //Dev
    options.Cookie.SameSite= SameSiteMode.None;
    options.Cookie.SecurePolicy= CookieSecurePolicy.Always;
});






builder.Services.AddIdentityCore<ApiUser>()
    .AddEntityFrameworkStores<apiv4.Data.ApiContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
