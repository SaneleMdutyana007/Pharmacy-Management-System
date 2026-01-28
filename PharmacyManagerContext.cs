using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.Users;
using Prototype.Server.Models.Users;
using System.Reflection.Emit;

namespace PharmacyManager.Data;

public class PharmacyDbContext : IdentityDbContext<UserModel>
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options)
        : base(options)
    {
    }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Pharmacy> PharmacyDetails { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<ActiveIngredient> ActiveIngredients { get; set; }
    public DbSet<MedicationActiveIngredient> MedicationIngredients { get; set; }
    public DbSet<DosageForm> DosageForms { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
    public DbSet<CustomerAllergy> CustomerAllergies { get; set; }
    public DbSet<PrescriptionRequest> PrescriptionRequests { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderMedication> OrderMedications { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserModel> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

       

        builder.Entity<UserModel>()
               .HasDiscriminator<string>("UserType")
               .HasValue<UserModel>("user")
               .HasValue<Admin>("admin")
               .HasValue<Customer>("customer")
               .HasValue<Pharmacist>("pharmacist")
               .HasValue<Manager>("manager")
               .HasValue<WalkIn>("walkin");


       

        builder.Entity<DosageForm>().HasData(
            new DosageForm { DosageFormId = 1, DosageFormName = "Tablet" },
            new DosageForm { DosageFormId = 2, DosageFormName = "Capsule" },
            new DosageForm { DosageFormId = 3, DosageFormName = "Suspension" },
            new DosageForm { DosageFormId = 4, DosageFormName = "Syrup" },
            new DosageForm { DosageFormId = 5, DosageFormName = "Lotion" },
            new DosageForm { DosageFormId = 6, DosageFormName = "Spray" },
            new DosageForm { DosageFormId = 7, DosageFormName = "Gel" },
            new DosageForm { DosageFormId = 8, DosageFormName = "Suppository" },
            new DosageForm { DosageFormId = 9, DosageFormName = "Injectable" },
            new DosageForm { DosageFormId = 10, DosageFormName = "Drops" },
            new DosageForm { DosageFormId = 11, DosageFormName = "IV Drip" }
        );

        builder.Entity<Customer>().HasData(
           new Customer { Id = "a1b2c3d4-e5f6-7890-abcd-1234567890ef".ToString(), Name = "Thabo", Surname = "Mokoena", SouthAfricanID= "8805125123087", PhoneNumber= "0823456789", Email= "thabo.mokoena@example.com" }
        );

        builder.Entity<CustomerAllergy>().HasData(
            new CustomerAllergy { Id = 1, ActiveIngredientId = 10, CustomerId = "a1b2c3d4-e5f6-7890-abcd-1234567890ef" }
        );



       
        // Order configuration
        builder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrdersId);
            entity.Property(e => e.OrdersId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderMedication configuration
        builder.Entity<OrderMedication>(entity =>
        {
            entity.HasKey(e => e.OrderMedicationId);
            entity.Property(e => e.OrderMedicationId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderMedications)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Medication)
                  .WithMany()
                  .HasForeignKey(e => e.MedicationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.NotificationId).ValueGeneratedOnAdd();

            // Relationships with separate foreign keys
            entity.HasOne(n => n.Medication)
                  .WithMany()
                  .HasForeignKey(n => n.MedicationId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(n => n.Doctor)
                  .WithMany()
                  .HasForeignKey(n => n.DoctorId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(n => n.Supplier)
                  .WithMany()
                  .HasForeignKey(n => n.SupplierId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(n => n.Order)
                  .WithMany()
                  .HasForeignKey(n => n.OrderId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);
        });




    }

}
