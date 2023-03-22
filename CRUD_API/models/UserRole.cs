using System.ComponentModel.DataAnnotations.Schema;

namespace Company_API.Models
{
    [Table("UserRole")]
    public class UserRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
