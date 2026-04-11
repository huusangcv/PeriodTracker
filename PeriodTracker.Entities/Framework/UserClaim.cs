using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodTracker.Entities.Framework
{
    [Table("FW_UserClaims")]
    public class UserClaim
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;
        public string? Role { get; set; }
        public string? Claims { get; set; }
    }
}
