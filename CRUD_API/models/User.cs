using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Company_API.Models
{
    [Table("User")]
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        [ForeignKey("UserRole")]
        public int UserRoleId { get; set; }
        public UserRole UserRole { get; set; }
        public string? RefreshToken { get; set; }
    }
}