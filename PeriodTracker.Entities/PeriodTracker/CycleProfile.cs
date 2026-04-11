using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PeriodTracker.Entities.Framework;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_CycleProfiles")]
    [Index(nameof(UserId), IsUnique = true)]
    public class CycleProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;

        public short TypicalCycleLengthDays { get; set; } = 28;

        public short TypicalPeriodLengthDays { get; set; } = 5;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
