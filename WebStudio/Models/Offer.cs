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
        
        [Required (ErrorMessage = "Поле обязательно")]
        [Remote("CheckCardNumber", "Validation", ErrorMessage = "Такого номера лота не существует")]
        [StringLength(11, MinimumLength = 0, ErrorMessage = "Длина номера лота от 0 до 12 знаков")]
        [DisplayName("Номер лота (на латинице)")]
        public string CardNumber { get; set; }
        public string CardId { get; set; }
        public virtual Card Card { get; set; }
        
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
        [DisplayName("Коммерческое предложение")]
        public IFormFile File { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        
        [DisplayName("Примечание")]
        public string Note { get; set; }
        
        public string UserId { get; set; }
        
    }
}