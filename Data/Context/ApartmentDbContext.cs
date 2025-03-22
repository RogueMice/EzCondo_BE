using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EzCondo_Data.Context;

public partial class ApartmentDbContext : DbContext
{
    public ApartmentDbContext()
    {
    }

    public ApartmentDbContext(DbContextOptions<ApartmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apartment> Apartments { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Citizen> Citizens { get; set; }

    public virtual DbSet<ElectricBill> ElectricBills { get; set; }

    public virtual DbSet<ElectricMeter> ElectricMeters { get; set; }

    public virtual DbSet<ElectricReading> ElectricReadings { get; set; }

    public virtual DbSet<Incident> Incidents { get; set; }

    public virtual DbSet<IncidentDetail> IncidentDetails { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationReceiver> NotificationReceivers { get; set; }

    public virtual DbSet<ParkingLot> ParkingLots { get; set; }

    public virtual DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PriceElectricTier> PriceElectricTiers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceImage> ServiceImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDevice> UserDevices { get; set; }

    public virtual DbSet<WaterBill> WaterBills { get; set; }

    public virtual DbSet<WaterMeter> WaterMeters { get; set; }

    public virtual DbSet<WaterReading> WaterReadings { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=ROGUE_MICE\\SQLEXPRESS;Initial Catalog=Apartment_db;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Apartmen__3213E83F77B5D6A6");

            entity.ToTable("Apartment");

            entity.HasIndex(e => e.ApartmentNumber, "UQ__Apartmen__2FFA3D2795CC3CC0").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Acreage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("acreage");
            entity.Property(e => e.ApartmentNumber)
                .HasMaxLength(50)
                .HasColumnName("apartment_number");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ResidentNumber).HasColumnName("resident_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Apartment_Users");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3213E83F502E4107");

            entity.ToTable("Booking");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Service");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_User");
        });

        modelBuilder.Entity<Citizen>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Pk_Citizen");

            entity.ToTable("Citizen");

            entity.HasIndex(e => e.No, "UQ__Citizen__3213D0812F45D660").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.BackImage)
                .HasMaxLength(255)
                .HasColumnName("backImage");
            entity.Property(e => e.DateOfExpiry).HasColumnName("dateOfExpiry");
            entity.Property(e => e.DateOfIssue).HasColumnName("dateOfIssue");
            entity.Property(e => e.FrontImage)
                .HasMaxLength(255)
                .HasColumnName("frontImage");
            entity.Property(e => e.No)
                .HasMaxLength(20)
                .HasColumnName("no");

            entity.HasOne(d => d.User).WithOne(p => p.Citizen)
                .HasForeignKey<Citizen>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Citizen_Users");
        });

        modelBuilder.Entity<ElectricBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Electric__3213E83FF29BFFBA");

            entity.ToTable("Electric_bill");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ReadingId).HasColumnName("reading_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalComsumption)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_comsumption");

            entity.HasOne(d => d.Customer).WithMany(p => p.ElectricBills)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_electric_bill_customer");

            entity.HasOne(d => d.Reading).WithMany(p => p.ElectricBills)
                .HasForeignKey(d => d.ReadingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_electric_bill_reading");
        });

        modelBuilder.Entity<ElectricMeter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Electric__3213E83F7A0BB12E");

            entity.ToTable("Electric_meters");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ApartmentId).HasColumnName("apartment_id");
            entity.Property(e => e.InstallationDate).HasColumnName("installation_date");
            entity.Property(e => e.MeterNumber)
                .HasMaxLength(50)
                .HasColumnName("meter_number");

            entity.HasOne(d => d.Apartment).WithMany(p => p.ElectricMeters)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_electric_meters_Apartment");
        });

        modelBuilder.Entity<ElectricReading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Electric__3213E83FE48B17C3");

            entity.ToTable("Electric_reading");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumption)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("consumption");
            entity.Property(e => e.CurrentElectricNumber)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("current_electric_number");
            entity.Property(e => e.ElectricMetersId).HasColumnName("electric_meters_id");
            entity.Property(e => e.PreElectricNumber)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pre_electric_number");
            entity.Property(e => e.ReadingDate)
                .HasColumnType("datetime")
                .HasColumnName("reading_date");

            entity.HasOne(d => d.ElectricMeters).WithMany(p => p.ElectricReadings)
                .HasForeignKey(d => d.ElectricMetersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_electric_reading_meters");
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Incident__3213E83FC95449D7");

            entity.ToTable("Incident");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.ReportedAt)
                .HasColumnType("datetime")
                .HasColumnName("reported_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Incidents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Incident_Users");
        });

        modelBuilder.Entity<IncidentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Incident__3213E83F5A7A3373");

            entity.ToTable("Incident_detail");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasColumnName("file_path");
            entity.Property(e => e.IncidentId).HasColumnName("incident_id");
            entity.Property(e => e.UploadAt)
                .HasColumnType("datetime")
                .HasColumnName("upload_at");

            entity.HasOne(d => d.Incident).WithMany(p => p.IncidentDetails)
                .HasForeignKey(d => d.IncidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentDetail_Incident");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83F0D3A0C15");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_CreatedBy");
        });

        modelBuilder.Entity<NotificationReceiver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83F9B1AC2A8");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ReadAt)
                .HasColumnType("datetime")
                .HasColumnName("read_at");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__notif__17036CC0");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__user___17F790F9");
        });

        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Parking___3213E83F2E1C5B1B");

            entity.ToTable("Parking_lot");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Spot)
                .HasMaxLength(50)
                .HasColumnName("spot");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Service).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParkingLot_Service");

            entity.HasOne(d => d.User).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ParkingLot_User");
        });

        modelBuilder.Entity<PasswordResetCode>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Password__1788CC4C41AA2FF0");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithOne(p => p.PasswordResetCode)
                .HasForeignKey<PasswordResetCode>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordResetCodes_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3213E83F66CB29AA");

            entity.ToTable("Payment");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.ElectricBillId).HasColumnName("electric_bill_id");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("method");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(50)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WaterBillId).HasColumnName("water_bill_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Payment_Booking");

            entity.HasOne(d => d.ElectricBill).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ElectricBillId)
                .HasConstraintName("FK_Payment_electric_bill");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_User");

            entity.HasOne(d => d.WaterBill).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WaterBillId)
                .HasConstraintName("FK_Payment_water_bill");
        });

        modelBuilder.Entity<PriceElectricTier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Price_el__3213E83F9EE0A1AE");

            entity.ToTable("Price_electric_tiers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.MaxKWh)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("max_kWh");
            entity.Property(e => e.MinKWh)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("min_kWh");
            entity.Property(e => e.PricePerKWh)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price_per_kWh");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3213E83F2D15F7BF");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Service__3213E83F552EB752");

            entity.ToTable("Service");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PriceOfMonth)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("priceOfMonth");
            entity.Property(e => e.PriceOfYear)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("priceOfYear");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TypeOfMonth)
                .HasDefaultValue(false)
                .HasColumnName("typeOfMonth");
            entity.Property(e => e.TypeOfYear)
                .HasDefaultValue(false)
                .HasColumnName("typeOfYear");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ServiceImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Service___3213E83F486F5B23");

            entity.ToTable("Service_Image");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ImgPath)
                .HasMaxLength(255)
                .HasColumnName("img_path");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceImages)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceImage_Service");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83FAED55E91");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__A1936A6BFF66211E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E61641FB776DE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(250)
                .HasColumnName("avatar");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .HasColumnName("gender");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_dev__3213E83F2E6AB943");

            entity.ToTable("user_device");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FcmToken)
                .HasMaxLength(255)
                .HasColumnName("fcm_token");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserDevices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserDevice_Users");
        });

        modelBuilder.Entity<WaterBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Water_bi__3213E83FB7F51EC7");

            entity.ToTable("Water_bill");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.PricePerM3)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price_per_m3");
            entity.Property(e => e.ReadingId).HasColumnName("reading_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalConsumption)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_consumption");

            entity.HasOne(d => d.Customer).WithMany(p => p.WaterBills)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_water_bill_customer");

            entity.HasOne(d => d.Reading).WithMany(p => p.WaterBills)
                .HasForeignKey(d => d.ReadingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_water_bill_reading");
        });

        modelBuilder.Entity<WaterMeter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Water_me__3213E83FBC1E855E");

            entity.ToTable("Water_meters");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ApartmentId).HasColumnName("apartment_id");
            entity.Property(e => e.InstallationDate).HasColumnName("installation_date");
            entity.Property(e => e.MeterNumber)
                .HasMaxLength(50)
                .HasColumnName("meter_number");

            entity.HasOne(d => d.Apartment).WithMany(p => p.WaterMeters)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_water_meters_Apartment");
        });

        modelBuilder.Entity<WaterReading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Water_re__3213E83F5DB21A32");

            entity.ToTable("Water_reading");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumption)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("consumption");
            entity.Property(e => e.CurrentWaterNumber)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("current_water_number");
            entity.Property(e => e.PreWaterNumber)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pre_water_number");
            entity.Property(e => e.ReadingDate)
                .HasColumnType("datetime")
                .HasColumnName("reading_date");
            entity.Property(e => e.WaterMetersId).HasColumnName("water_meters_id");

            entity.HasOne(d => d.WaterMeters).WithMany(p => p.WaterReadings)
                .HasForeignKey(d => d.WaterMetersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_water_reading_meters");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
