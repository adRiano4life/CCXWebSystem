using System.ComponentModel.DataAnnotations;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class CreatePositionViewModel
    {
        [Required(ErrorMessage = "Это поле обязательно для заполнение")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Единица измерения")]
        public string Measure { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Количество")]
        public float Amount { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Цена за единицу товара")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Срок поставки")]
        public string DeliveryTime { get; set; }
        
        [Display(Name = "Условия поставки")]
        public string DeliveryTerms { get; set; }
        public string CardId { get; set; }
        public Card Card { get; set; }
        
    }
}