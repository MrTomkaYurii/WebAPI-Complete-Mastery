using System.ComponentModel.DataAnnotations;

namespace FluentValidations.NET8.Requests;

public class UserRegistrationRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}


//public class UserRegistrationRequest
//{
//    [Required(ErrorMessage = "First name is required.")]
//    public string? FirstName { get; set; }

//    [Required(ErrorMessage = "Last name is required.")]
//    public string? LastName { get; set; }

//    [Required(ErrorMessage = "Email is required.")]
//    [EmailAddress(ErrorMessage = "Invalid email address.")]
//    public string? Email { get; set; }

//    [Required(ErrorMessage = "Password is required.")]
//    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
//    public string? Password { get; set; }

//    [Required(ErrorMessage = "Please confirm your password.")]
//    [Compare("Password", ErrorMessage = "Passwords do not match.")]
//    public string? ConfirmPassword { get; set; }
//}