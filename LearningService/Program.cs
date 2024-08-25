using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAzureWebAppDiagnostics();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.UseCredential(new DefaultAzureCredential());

    clientBuilder.ConfigureDefaults(builder.Configuration.GetSection("AzureDefaults"));
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

var initialScopes = builder.Configuration["AzureAdB2C:Scopes"]?.Split(' ');

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "Learning Service");

app.MapGet("api/authorized", () => "Authorized!")
    .WithOpenApi()
    .WithName("Authorized")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status401Unauthorized)
    .RequireAuthorization();

await app.RunAsync();
