using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class DetailCardViewModel
    {
        public Card Card { get; set; }
        public List<User> Users { get; set; }
        public string CardId { get; set; }
        public string UserId { get; set; }
    }
}