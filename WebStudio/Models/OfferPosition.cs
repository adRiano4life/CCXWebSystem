using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebStudio.Models
{
    public class OfferPosition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CodTNVED { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string Name { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string Measure { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public float Amount { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public decimal UnitPrice { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string Currency { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public decimal TotalPrice { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string DeliveryTerms { get; set; }
        public string Comment { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string DeliveryTime { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string PaymentTerms { get; set; }
        [Required (ErrorMessage = "Поле обязательно")]
        public string DeliveryCity { get; set; }

        public string OfferId { get; set; }
        public virtual Offer Offer { get; set; }
    }
}