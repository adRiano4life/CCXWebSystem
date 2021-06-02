using System.ComponentModel.DataAnnotations;

namespace WebStudio.ViewModels
{
    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "Поле обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес эл.почты")]
        public string Email { get; set; }
        public string Website { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Поле обязательно")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Добавьте тег")]
        public string Tags { get; set; }
    }
}