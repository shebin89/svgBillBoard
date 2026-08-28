using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
       DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<DevicePairing> DevicePairings => Set<DevicePairing>();
        public DbSet<DeviceCredential> DeviceCredentials => Set<DeviceCredential>();
        public DbSet<Media> Media => Set<Media>();
        public DbSet<Playlist> Playlists => Set<Playlist>();
        public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();
        public DbSet<PlaylistAssignment> PlaylistAssignments => Set<PlaylistAssignment>();
        public DbSet<PlaylistSchedule> PlaylistSchedules => Set<PlaylistSchedule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organizations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(255);

                entity.Property(x => x.Phone)
                    .HasMaxLength(30);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.Code
                })
                .IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasMaxLength(30);

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.Email
                })
                .IsUnique();

                entity.HasOne(x => x.Organization)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.Code
                })
                .IsUnique();

                entity.HasOne(x => x.Organization)
                    .WithMany(x => x.Roles)
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasKey(x => new
                {
                    x.UserId,
                    x.RoleId
                });

                entity.HasOne(x => x.User)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Code)
                    .HasMaxLength(50);

                entity.Property(x => x.AddressLine1)
                    .HasMaxLength(250);

                entity.Property(x => x.AddressLine2)
                    .HasMaxLength(250);

                entity.Property(x => x.City)
                    .HasMaxLength(100);

                entity.Property(x => x.State)
                    .HasMaxLength(100);

                entity.Property(x => x.PostalCode)
                    .HasMaxLength(20);

                entity.Property(x => x.Country)
                    .HasMaxLength(100);

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.Code
                })
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL");

                entity.HasOne(x => x.Organization)
                    .WithMany(x => x.Locations)
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Devices");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.DeviceIdentifier)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DeviceCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DeviceType)
                    .HasMaxLength(50);

                entity.Property(x => x.Platform)
                    .HasMaxLength(50);

                entity.Property(x => x.AppVersion)
                    .HasMaxLength(50);

                entity.Property(x => x.Model)
                    .HasMaxLength(150);

                entity.Property(x => x.Manufacturer)
                    .HasMaxLength(150);

                entity.Property(x => x.SerialNumber)
                    .HasMaxLength(150);

                entity.Property(x => x.MacAddress)
                    .HasMaxLength(50);

                entity.Property(x => x.IpAddress)
                    .HasMaxLength(50);

                entity.Property(x => x.IsOnline)
                    .IsRequired();

                entity.HasIndex(x => x.DeviceIdentifier)
                    .IsUnique();

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.DeviceCode
                })
                .IsUnique();

                entity.HasOne(x => x.Organization)
                    .WithMany()
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Location)
                    .WithMany(x => x.Devices)
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DevicePairing>(entity =>
            {
                entity.ToTable("DevicePairings");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PairingCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(x => x.PairingCode)
                    .IsUnique();

                entity.HasOne(x => x.Device)
                    .WithMany()
                    .HasForeignKey(x => x.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DeviceCredential>(entity =>
            {
                entity.ToTable("DeviceCredentials");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasOne(x => x.Device)
                    .WithMany()
                    .HasForeignKey(x => x.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.ToTable("Playlists");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.Name
                })
                .IsUnique();

                entity.HasOne<Organization>()
                    .WithMany()
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Playlist)
                    .HasForeignKey(x => x.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlaylistItem>(entity =>
            {
                entity.ToTable("PlaylistItems");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.DurationSeconds)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.PlaylistId,
                    x.DisplayOrder
                })
                .IsUnique();

                entity.HasOne(x => x.Playlist)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Media)
                    .WithMany()
                    .HasForeignKey(x => x.MediaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<PlaylistAssignment>(entity =>
            {
                entity.ToTable("PlaylistAssignments");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.LocationId,
                    x.PlaylistId
                })
                .IsUnique();

                entity.HasOne(x => x.Playlist)
                    .WithMany()
                    .HasForeignKey(x => x.PlaylistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Location)
                    .WithMany()
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Organization>()
                    .WithMany()
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<PlaylistSchedule>(entity =>
            {
                entity.ToTable("PlaylistSchedules");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DaysOfWeek)
                    .IsRequired();

                entity.Property(x => x.Priority)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.OrganizationId,
                    x.LocationId,
                    x.Priority
                });

                entity.HasOne(x => x.Playlist)
                    .WithMany()
                    .HasForeignKey(x => x.PlaylistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Location)
                    .WithMany()
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Organization>()
                    .WithMany()
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
