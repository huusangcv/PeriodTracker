using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker.Entities.PeriodTracker
{
    [Table("PT_Symptoms")]
    [Index(nameof(Code), IsUnique = true)]
    public class Symptom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        [MaxLength(50)]
        public string? Category { get; set; }

        public short SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
