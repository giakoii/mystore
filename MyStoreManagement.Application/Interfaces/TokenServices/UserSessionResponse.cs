namespace MyStoreManagement.Application.Interfaces.TokenServices;

public class UserSessionResponse
{
    public int UserId { get; set; }
    
    public string Name { get; set; }
    
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
}