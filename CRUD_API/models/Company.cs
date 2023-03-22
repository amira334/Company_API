using System.ComponentModel.DataAnnotations.Schema;

namespace Company_API.Models
{
    [Table("Company")]
    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set;}
        public string CompanyDescription { get; set;}
        public string CompanyPhone { get; set;}
        public string CompanyEmail { get; set;}

    }
}
