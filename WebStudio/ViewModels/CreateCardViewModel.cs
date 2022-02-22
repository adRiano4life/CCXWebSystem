using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class CreateCardViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Номер лота")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Название лота")]
        public string Name { get; set; }

        public decimal StartSumm { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Дата окончания приема заявок")]
        public DateTime DateOfAcceptingEnd { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Дата начала торгов")]
        public DateTime DateOfAuctionStart { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Инициатор лота")]
        public string Initiator { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Тип торгов")]
        public string AuctionType { get; set; }
        
        [NotMapped] 
        public List<IFormFile> Files { get; set; }
    }
}