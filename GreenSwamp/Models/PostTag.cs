using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwamp.Models
{
    [Table("post_tags")]
    public class PostTag
    {
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("tag_id")]
        public int TagId { get; set; }

        // Навигационные свойства
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}