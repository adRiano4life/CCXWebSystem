using System.ComponentModel.DataAnnotations;

namespace WebStudio.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required] 
        public string Email { get; set; }
    }
}