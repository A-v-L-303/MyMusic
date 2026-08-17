namespace MyMusic.Infrastructure.ExternalServices.Keycloak;

public sealed record KeycloakTokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
