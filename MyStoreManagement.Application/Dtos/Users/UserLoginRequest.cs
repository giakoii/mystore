namespace MyStoreManagement.Application.Dtos.Users;

public record UserLoginRequest(string PhoneNumber, string? Password);