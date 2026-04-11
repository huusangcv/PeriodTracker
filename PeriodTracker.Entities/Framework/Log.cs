using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeriodTracker.Entities.Framework
{
    [Table("FW_Logs")]
    public class Log
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string LogLevel { get; set; } = default!;
        public string? Message { get; set; }
        public string? Exception { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
