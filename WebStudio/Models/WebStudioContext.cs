using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebStudio.Models
{
    public class WebStudioContext : IdentityDbContext
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CardPosition> Positions { get; set; }
        public DbSet<Request> Requests { get; set; }

        public WebStudioContext(DbContextOptions options) : base(options)
        {
        }
    }
}