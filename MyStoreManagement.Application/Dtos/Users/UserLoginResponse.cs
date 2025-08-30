namespace MyStoreManagement.Application.Dtos.Users;

public class UserLoginResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleName { get; set; }
}