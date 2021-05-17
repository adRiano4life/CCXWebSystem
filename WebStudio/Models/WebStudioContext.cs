using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebStudio.Models
{
    public class WebStudioContext : IdentityDbContext
    {
        public DbSet<Node> Nodes { get; set; }

        public WebStudioContext(DbContextOptions options) : base(options)
        {
        }
    }
}