using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Sona.Application.Abstractions;
using Sona.Application.Auth;
using Sona.Application.Configuration;
using Sona.Application.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;

namespace Sona.Api;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddOptions<SpotifyOptions>()
            .Bind(configuration.GetSection("Spotify"))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.BaseUrl) &&
                    !string.IsNullOrWhiteSpace(options.AccountsBaseUrl) &&
                    !string.IsNullOrWhiteSpace(options.RedirectUri) &&
                    !string.IsNullOrWhiteSpace(options.FrontendUrl),
                "Spotify options are missing required values.")
            .ValidateOnStart();

        services.AddScoped<SpotifyConnectionService>();
        services.AddScoped<SpotifyAccountService>();

        services.AddSingleton<DevelopmentSpotifyTokenStore>();
        services.AddScoped<ISpotifyConnectionGateway, SpotifyAuthorizationService>();
        services.AddScoped<ISpotifyProfileGateway, SpotifyProfileGateway>();
        services.AddScoped<ISpotifyPlaylistGateway, SpotifyPlaylistGateway>();
        services.AddHttpClient<SpotifyClient>((sp, client) =>
        {
            var spotify = sp.GetRequiredService<IOptions<SpotifyOptions>>().Value;
            client.BaseAddress = new Uri(spotify.BaseUrl, UriKind.Absolute);
        });
        services.AddHttpClient<SpotifyAuthClient>((sp, client) =>
        {
            var spotify = sp.GetRequiredService<IOptions<SpotifyOptions>>().Value;
            client.BaseAddress = new Uri(spotify.AccountsBaseUrl, UriKind.Absolute);
        });
    }
}
