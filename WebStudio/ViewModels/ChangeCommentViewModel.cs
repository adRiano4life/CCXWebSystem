using System;
using System.ComponentModel.DataAnnotations;

namespace WebStudio.ViewModels
{
    public class ChangeCommentViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        public string Message { get; set; }
        public DateTime DateOfChange { get; set; }
    }
}