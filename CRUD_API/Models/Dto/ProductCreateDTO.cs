using System.ComponentModel.DataAnnotations;

namespace Company_API.Models.DTO
{
    public class ProductCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int Stock { get; set; }
        [Required]
        public int CompanyId { get; set; }
    }
}
