using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebStudio.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Запомнить")] 
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}