using MessengerModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MessengerServer
{
    internal class MessengerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public MessengerDbContext()
        {
            if (Database.EnsureCreated())
            {
                Statuses!.AddRange([
                    new Status() { Name = "Offline" },
                    new Status() { Name = "Online" }
                ]);
                SaveChanges();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("Default");

            optionsBuilder.UseSqlServer(config);
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(item =>
            {
                item.HasIndex(item => item.Login).IsUnique();
                item.HasMany(item => item.BlockedUsers)
                    .WithMany();
                item.Property(item => item.Login).HasMaxLength(255);
                item.Property(item => item.Password).HasMaxLength(255);
            });

            modelBuilder.Entity<Message>(item =>
            {
                item.HasOne(item => item.Sender)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
                item.HasOne(item => item.Recipient)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Status>(item =>
            {
                item.HasIndex(item => item.Name).IsUnique();
                item.Property(item => item.Name).HasMaxLength(255);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
