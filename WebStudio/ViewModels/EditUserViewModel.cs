﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebStudio.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Имя пользователя")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Фамилия пользователя")]
        [DataType(DataType.Text)]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Номер телефона")]
        [DataType(DataType.Text)]
        public string PhoneNumber { get; set; }
        
        public string AvatarPath { get; set; }
        public IFormFile File { get; set; }
    }
}