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
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<AuctionResult> AuctionResults { get; set; }

        public WebStudioContext(DbContextOptions options) : base(options)
        {
        }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().HasData(new User {Id = "1", Name = "Jake", Surname = "Billson", AvatarPath = $"/Images/Avatars/defaultavatar.jpg", Email = "Jake_Billson@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "2", Name = "Pol", Surname = "Dou", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Pol_Dou@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "3", Name = "Helen", Surname = "Merker", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Helen_Merker@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "4", Name = "Jhon", Surname = "Sohnson", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Jhon_Sohnson@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "5", Name = "Phil", Surname = "Madison", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Phill_Madison@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "6", Name = "Mark", Surname = "Takeson", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Mark_Takeson@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "7", Name = "Max", Surname = "Carlson", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Max_Carlson@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "8", Name = "Caren", Surname = "Jameson", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Caren_Jameson@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "9", Name = "July", Surname = "Pablich", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "July_Pablich@gmail.com"});
            modelBuilder.Entity<User>().HasData(new User {Id = "10", Name = "Tad", Surname = "Wilkerson", AvatarPath = "/Images/Avatars/defaultavatar.jpg", Email = "Tad_Wilkerson@gmail.com"});
            
            

            
        }
    }
}