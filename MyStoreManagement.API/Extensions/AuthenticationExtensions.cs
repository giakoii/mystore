using Microsoft.AspNetCore;
using MyStoreManagement.Infrastructure.Contexts;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace MyStoreManagement.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<MyStoreManagementContext>();
            })
            .AddServer(options =>
            {
                ConfigureEndpoints(options);
                ConfigureFlows(options);
                ConfigureScopes(options);
                ConfigureCertificates(options);
                ConfigureTokenLifetime(options);
                ConfigureAspNetCore(options);
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthorization();
        services.AddDataProtection();

        return services;
    }

    private static void ConfigureEndpoints(OpenIddictServerBuilder options)
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetIntrospectionEndpointUris("/connect/introspect");
        options.SetUserInfoEndpointUris("/connect/userinfo");
        options.SetEndSessionEndpointUris("/connect/logout");
        options.SetAuthorizationEndpointUris("/connect/authorize");
    }

    private static void ConfigureFlows(OpenIddictServerBuilder options)
    {
        options.AllowCustomFlow("google");
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        options.AllowClientCredentialsFlow();
        options.AllowCustomFlow("logout");
        options.AllowCustomFlow("external");
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

        options.AcceptAnonymousClients();
    }

    private static void ConfigureScopes(OpenIddictServerBuilder options)
    {
        options.RegisterScopes(OpenIddictConstants.Scopes.OfflineAccess);
        options.RegisterScopes(
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles);
    }

    private static void ConfigureCertificates(OpenIddictServerBuilder options)
    {
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
    }

    private static void ConfigureTokenLifetime(OpenIddictServerBuilder options)
    {
        options.UseReferenceAccessTokens();
        options.UseReferenceRefreshTokens();
        options.DisableAccessTokenEncryption();

        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(60));
        options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(120));
    }

    private static void ConfigureAspNetCore(OpenIddictServerBuilder options)
    {
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .DisableTransportSecurityRequirement();
    }
}