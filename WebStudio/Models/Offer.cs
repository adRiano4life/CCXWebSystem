using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;

namespace WebStudio.Models
{
    public class Offer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Remote("CheckCardNumber", "Validation", ErrorMessage = "Такого номера лота не существует")]
        [StringLength(11, MinimumLength = 0, ErrorMessage = "Длина номера лота от 0 до 12 знаков")]
        public string CardId { get; set; }
        public virtual Card Card { get; set; }
        
        [Required (ErrorMessage = "Поле обязательно")]
        public string SupplierName { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public DateTime DateOfIssue { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string Number { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
        public string Path { get; set; }

        public virtual  List<OfferPosition> Positions { get; set; }
        
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}