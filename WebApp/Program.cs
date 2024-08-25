using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Security.Claims;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var initialScopes = builder.Configuration["ApiScopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    //options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddHttpClient<ILearningServiceClient, LearningServiceClient>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddAzureClients(configureClients =>
{
    configureClients.UseCredential(new DefaultAzureCredential());
});

builder.Services.AddHttpClient();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", () => "Web App");

app.MapGet("/user", (ClaimsPrincipal principal) =>
{
    return principal?.Identity?.Name;
}).RequireAuthorization();

app.MapGet("/api/learning-service/authorized", (ILearningServiceClient learningService) =>
{
    return learningService.GetAuthorizedAsync();
})
    .WithOpenApi()
    .WithTags("Learning Service")
    .WithName("Learning Service Authorized")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status401Unauthorized)
    .RequireAuthorization();

app.MapGet("/login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new AuthenticationProperties
    {
        RedirectUri = returnUrl
    };

    await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/register", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new AuthenticationProperties
    {
        RedirectUri = returnUrl
    };

    await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authenticationProperties);
});

await app.RunAsync();