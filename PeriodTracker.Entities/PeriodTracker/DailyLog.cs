using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PeriodTracker.Entities.Framework;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_DailyLogs")]
    [Index(nameof(UserId), nameof(LogDate), IsUnique = true)]
    public class DailyLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;

        [Column(TypeName = "date")]
        public DateTime LogDate { get; set; }

        [MaxLength(30)]
        public string? FlowLevel { get; set; }

        [MaxLength(30)]
        public string? Mood { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
