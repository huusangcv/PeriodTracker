using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_DailyLogSymptoms")]
    [Index(nameof(DailyLogId), nameof(SymptomId), IsUnique = true)]
    public class DailyLogSymptom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public int DailyLogId { get; set; }

        [ForeignKey(nameof(DailyLogId))]
        public virtual DailyLog DailyLog { get; set; } = default!;

        [Required]
        public int SymptomId { get; set; }

        [ForeignKey(nameof(SymptomId))]
        public virtual Symptom Symptom { get; set; } = default!;

        public byte? Severity { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
