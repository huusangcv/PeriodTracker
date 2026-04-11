using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PeriodTracker.Entities.Framework;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_Cycles")]
    [Index(nameof(UserId), nameof(StartDate), IsUnique = true)]
    [Index(nameof(UserId), nameof(EndDate))]
    public class Cycle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [MaxLength(20)]
        public string Source { get; set; } = "Manual";

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
