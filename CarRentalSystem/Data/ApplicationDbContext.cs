using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Models;

namespace CarRentalSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasDefaultValue("Customer");
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NICNumber).IsRequired().HasMaxLength(20);
            });

            // Configure Car entity
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(e => e.CarID);
                entity.Property(e => e.CarName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CarModel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Seats).IsRequired().HasDefaultValue(4);
                entity.Property(e => e.DailyRate).IsRequired().HasColumnType("decimal(10,2)").HasDefaultValue(50.00m);
                entity.Property(e => e.ImageUrl).HasMaxLength(200);
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingID);
                entity.Property(e => e.TotalCost).HasColumnType("decimal(10,2)");

                // Configure relationships
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.CustomerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Car)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.CarID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    Password = "admin123",
                    Role = "Admin",
                    FullName = "System Administrator",
                    Email = "admin@carrentalsystem.com",
                    PhoneNumber = "+1234567890",
                    Address = "123 Admin Street, Admin City",
                    NICNumber = "ADMIN123456789"
                }
            );

            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    CarID = 1,
                    CarName = "Camry",
                    Brand = "Toyota",
                    CarModel = "2023",
                    Seats = 5,
                    DailyRate = 45.00m,
                    ImageUrl = "https://stimg.cardekho.com/images/carexteriorimages/930x620/Toyota/Camry/11344/1733916451269/front-left-side-47.jpg?imwidth=890&impolicy=resize",
                    IsAvailable = true
                },
                new Car
                {
                    CarID = 2,
                    CarName = "Civic",
                    Brand = "Honda",
                    CarModel = "2023",
                    Seats = 5,
                    DailyRate = 40.00m,
                    ImageUrl = "https://media.ed.edmunds-media.com/honda/civic/2026/oem/2026_honda_civic_sedan_si_fq_oem_1_815.jpg",
                    IsAvailable = true
                },
                new Car
                {
                    CarID = 3,
                    CarName = "X5",
                    Brand = "BMW",
                    CarModel = "2023",
                    Seats = 7,
                    DailyRate = 85.00m,
                    ImageUrl = "https://www.topgear.com/sites/default/files/2024/05/P90489757_highRes_the-new-bmw-x5-xdriv_0.jpg?w=1784&h=1004",
                    IsAvailable = true
                }
            );
        }
    }
}
