using System.ComponentModel.DataAnnotations;

namespace WebStudio.ViewModels
{
    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "Поле обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string Email { get; set; }
        public string Website { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string Tags { get; set; }
    }
}