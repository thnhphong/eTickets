using System.ComponentModel.DataAnnotations;
namespace eTickets.Models
{
    public class EmailMarketingModel
    {
        [Required(ErrorMessage = "Vui long nhap email")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Vui long nhap ho ten")]
        public string Name { get; set; }
    }
}
