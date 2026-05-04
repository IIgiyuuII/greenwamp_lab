using GreenSwamp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace GreenSwamp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Auth> Auths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка составного первичного ключа для PostTag
            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            // Настройка уникального индекса для взаимодействий
            modelBuilder.Entity<Interaction>()
                .HasIndex(i => new { i.UserId, i.PostId, i.InteractionType })
                .IsUnique();

            // Настройка связи один-к-одному между Post и Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Post)
                .WithOne(p => p.Event)
                .HasForeignKey<Event>(e => e.PostId);

            // Настройка само-ссылающейся связи для реплаев
            modelBuilder.Entity<Post>()
                .HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict);

            // Представление для трендовых тегов
            modelBuilder.Entity<TrendingPond>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("trending_ponds");
            });
        }

        public DbSet<TrendingPond> TrendingPonds { get; set; }
    }

    [Keyless]
    public class TrendingPond
    {
        [Column("tag_id")]
        public int TagId { get; set; }

        [Column("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [Column("recent_posts")]
        public int RecentPosts { get; set; }
    }
}