using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WebStudio.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string AvatarPath { get; set; }
        
        [NotMapped] 
        public IFormFile File { get; set; }
    }
}