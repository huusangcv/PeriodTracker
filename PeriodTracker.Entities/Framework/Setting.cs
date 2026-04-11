using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodTracker.Entities.Framework
{
    [Table("FW_Settings")]
    public class Setting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Value { get; set; }
    }
}
