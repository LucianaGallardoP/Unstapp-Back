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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.PostDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.Content).IsRequired();

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

                entity.HasIndex(e => new {e.PostId, e.UserId })
                    .IsUnique();
            });


        }
    }
}
