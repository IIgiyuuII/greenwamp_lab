using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwamp.Models
{
    [Table("tags")]
    public class Tag
    {
        [Key]
        [Column("tag_id")]
        public int TagId { get; set; }

        [Column("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("usage_count")]
        public int UsageCount { get; set; }

        // Навигационные свойства
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}