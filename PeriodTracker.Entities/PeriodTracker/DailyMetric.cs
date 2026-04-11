using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PeriodTracker.Entities.Framework;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_DailyMetrics")]
    [Index(nameof(UserId), nameof(MeasuredDate), nameof(MetricType), IsUnique = true)]
    [Index(nameof(UserId), nameof(MetricType), nameof(MeasuredDate))]
    public class DailyMetric
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;

        [Column(TypeName = "date")]
        public DateTime MeasuredDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string MetricType { get; set; } = default!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DecimalValue { get; set; }

        [MaxLength(20)]
        public string? Unit { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
