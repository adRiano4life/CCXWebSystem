using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebStudio.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Имя пользователя")]
        [DataType(DataType.Text)]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Фамилия пользователя")]
        [DataType(DataType.Text)]
        public string UserSurname { get; set; }
        
        public string AvatarPath { get; set; }
        public IFormFile File { get; set; }
    }
}