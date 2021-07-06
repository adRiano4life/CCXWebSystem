using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DisplayName("Номер лота (на латинице)")]
        public string CardNumber { get; set; }
        
        [Required (ErrorMessage = "Поле обязательно")]
        [DisplayName("Поставщик")]
        public string SupplierName { get; set; }
        
        [Required (ErrorMessage = "Поле обязательно")]
        [DisplayName("Дата КП")]
        public DateTime DateOfIssue { get; set; }
        
        [Required (ErrorMessage = "Поле обязательно")]
        [DisplayName("Номер КП")]
        public string Number { get; set; }
        
        [NotMapped]
        [DisplayName("Загрузить КП")]
        public IFormFile File { get; set; }
        public string Path { get; set; }

        public virtual  List<OfferPosition> Positions { get; set; }
        
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}