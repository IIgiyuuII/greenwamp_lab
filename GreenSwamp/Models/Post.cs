using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwamp.Models
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("post_type")]
        public string PostType { get; set; } = "text";

        [Column("media_url")]
        public string? MediaUrl { get; set; }

        [Column("media_type")]
        public string? MediaType { get; set; }

        [Column("alt_text")]
        public string? AltText { get; set; }

        [Column("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("parent_post_id")]
        public int? ParentPostId { get; set; }

        // Навигационные свойства
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ParentPostId")]
        public virtual Post? ParentPost { get; set; }

        public virtual ICollection<Post> Replies { get; set; } = new List<Post>();
        public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public virtual Event? Event { get; set; }
    }
}