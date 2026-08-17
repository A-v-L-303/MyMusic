namespace MyMusic.Application.Features.Verwaltung.Admin.ResponseDtos;

public sealed record UserResponse(Guid Id, string Username, string Email, string Role);
