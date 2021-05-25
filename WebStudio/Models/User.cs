using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WebStudio.Models
{
    public class User : IdentityUser
    {
        public string UserSurname { get; set; }
        public string AvatarPath { get; set; }
        
        [NotMapped] 
        public IFormFile File { get; set; }
    }
}