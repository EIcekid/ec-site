namespace EcSite.Api.DTOs.Users;

public record MeDto(int Id, string Email, string Name, string Role, int Points);
public record UpdateProfileRequest(string Name);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
