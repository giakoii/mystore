using System.ComponentModel.DataAnnotations;

namespace MyStoreManagement.Application.Dtos.Users;

public class UserRoleSelectRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
}