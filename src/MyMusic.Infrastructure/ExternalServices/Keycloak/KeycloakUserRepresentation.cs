namespace MyMusic.Infrastructure.ExternalServices.Keycloak;

public sealed record KeycloakUserRepresentation(string Id, string Username, string? Email);
