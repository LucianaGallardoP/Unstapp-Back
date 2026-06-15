using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Career> Careers => Set<Career>();
        public DbSet<UserCareer> UserCareers => Set<UserCareer>();
        public DbSet<PostCareer> PostCareers => Set<PostCareer>();
        public DbSet<Faculty> Faculties => Set<Faculty>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<UserFollow> UserFollow => Set<UserFollow>();
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasDbFunction(typeof(PostgresDbFunctions)
                .GetMethod(nameof(PostgresDbFunctions.Unaccent), new[] { typeof(string) })!)
                .HasName("unaccent");

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(u => u.Bio).HasMaxLength(500);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.PostId);
                entity.Property(e => e.Content).HasMaxLength(500);
                entity.Property(e => e.PostDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.Category).HasConversion<string>();
                entity.Property(p => p.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(300);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.LikeId);

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PostId, e.UserId })
                    .IsUnique();
            });

            modelBuilder.Entity<Career>(entity =>
            {
                entity.HasKey(c => c.CareerId);

                entity.Property(c => c.Name)
                    .IsRequired();

                entity.HasOne(c => c.Faculty)
                    .WithMany(f => f.Careers)
                    .HasForeignKey(c => c.FacultyId);
            });

            modelBuilder.Entity<UserCareer>(entity =>
            {
                entity.HasKey(uc => new { uc.UserId, uc.CareerId });

                entity.HasOne(uc => uc.User)
                    .WithMany(u => u.UserCareers)
                    .HasForeignKey(uc => uc.UserId);

                entity.HasOne(uc => uc.Career)
                    .WithMany(c => c.UserCareers)
                    .HasForeignKey(uc => uc.CareerId);
            });

            modelBuilder.Entity<PostCareer>(entity =>
            {
                entity.HasKey(pc => new { pc.PostId, pc.CareerId });

                entity.HasOne(pc => pc.Post)
                    .WithMany(p => p.PostCareers)
                    .HasForeignKey(pc => pc.PostId);

                entity.HasOne(pc => pc.Career)
                    .WithMany(c => c.PostCareers)
                    .HasForeignKey(pc => pc.CareerId);
            });

            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(f => f.FacultyId);

                entity.Property(f => f.Name)
                .IsRequired();
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);

                entity.Property(n => n.ActorUserName)
                    .IsRequired();

                entity.Property(n => n.ActionType)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(n => n.IsPriority)
                    .HasDefaultValue(false);

                entity.Property(n => n.IsRead)
                    .HasDefaultValue(false);

                entity.Property(n => n.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(n => n.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(n => n.RecipientUser)
                    .WithMany(u => u.ReceivedNotifications)
                    .HasForeignKey(n => n.RecipientUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.ActorUser)
                    .WithMany(u => u.SentNotifications)
                    .HasForeignKey(n => n.ActorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Post)
                    .WithMany()
                    .HasForeignKey(n => n.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserFollow>(entity =>
            {
                entity.HasKey(uf => new { uf.FollowerUserId, uf.FollowedUserId });

                entity.HasOne(uf => uf.FollowerUser)
                    .WithMany(u => u.Following)
                    .HasForeignKey(uf => uf.FollowerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(uf => uf.FollowedUser)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(uf => uf.FollowedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(uf => uf.FollowedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<CalendarEvent>(entity =>
            {
                entity.HasKey(e => e.CalendarEventId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<string>().IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
