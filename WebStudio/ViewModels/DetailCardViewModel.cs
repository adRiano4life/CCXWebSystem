using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class DetailCardViewModel
    {
        public Card Card { get; set; }
        public List<User> Users { get; set; }
    }
}