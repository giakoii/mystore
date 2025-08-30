using System.ComponentModel.DataAnnotations;

namespace MyStoreManagement.Application.Dtos.Users;

public record UserCreateRequest(
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    string PhoneNumber,

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    string FullName
);