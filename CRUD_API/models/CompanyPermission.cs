using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Company_API.Models
{
    [Table("CompanyPermission")]
    public class CompanyPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }
        public int CompanyId { get; set; }
        [ForeignKey("CompnayId")]
        [ValidateNever]
        public Company Company { get; set; }
    }
}
