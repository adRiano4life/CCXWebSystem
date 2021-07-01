using System.ComponentModel.DataAnnotations;

namespace WebStudio.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно")]
        [Display(Name = "Новый пароль")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}