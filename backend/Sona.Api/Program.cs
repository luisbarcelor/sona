using Sona.Infrastructure.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;
using Sona.Infrastructure.Spotify.Configuration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var spotifyOptions = builder.Configuration.GetSection("Spotify").Get<SpotifyOptions>() ?? new SpotifyOptions();
builder.Services.AddSingleton(spotifyOptions);
builder.Services.AddSingleton<DevelopmentSpotifyTokenStore>();
builder.Services.AddScoped<SpotifyAuthorizationService>();
builder.Services.AddHttpClient<SpotifyClient>(client =>
{
    var baseUrl = builder.Configuration["Spotify:BaseUrl"] ?? "https://api.spotify.com";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<SpotifyAuthClient>(client =>
{
    var baseUrl = builder.Configuration["Spotify:AccountsBaseUrl"] ?? "https://accounts.spotify.com";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();
