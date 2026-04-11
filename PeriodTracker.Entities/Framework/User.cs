using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodTracker.Entities.Framework
{
    [Table("FW_Users")]
    [Index(nameof(UserName), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(50)]
        public string UserName { get; set; } = null!;
        [MaxLength(500)]
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        [MaxLength(500)]
        public string? DisplayName { get; set; }
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsSuperAdmin { get; set; } = false;
        public bool Active { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool Deleted { get; set; } = false;
      
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasData(
                    new User
                    {
                        Id = 1,
                        UserName = "admin",
                        Email = "seedu@gmail.com",
                        Active = true,
                        AvatarUrl = "https://img.freepik.com/premium-photo/animal-bird-icon-3d-rendering-isolated-background_150525-3358.jpg",
                        Deleted = false,
                        DisplayName = "Administrator",
                        IsSuperAdmin = true,
                        Salt = "Ps7QUW1K",
                        Password = "FD5AFC1A4DB3A52ABD8A09425B3FB0AB8BB52E23", //123456
                        Role = "admin"
                    }
                );
            });

        }
    }
}
