using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OrderManagementSystem_DAL.Entities;

public partial class OrderManagementDbContext : DbContext
{
    public OrderManagementDbContext()
    {
    }

    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agency> Agencies { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<SalesDetail> SalesDetails { get; set; }

    public virtual DbSet<SalesMaster> SalesMasters { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=SHWETA-PC;Database=OrderManagementDB;User Id=sa;Password=sr@1911;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Agencies__3214EC071A70E932");

            entity.Property(e => e.AgencyName).HasMaxLength(200);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.Barcode, "UQ_Items_Barcode").IsUnique();

            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.Rate).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Agency).WithMany(p => p.Items)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("FK_Items_Agencies");
        });

        modelBuilder.Entity<SalesDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesDet__3214EC0725752DE5");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.Rate).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Agency).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("FK_SalesDetails_Agencies");

            entity.HasOne(d => d.Item).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_SalesDetails_Items");

            entity.HasOne(d => d.Sales).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.SalesId)
                .HasConstraintName("FK__SalesDeta__Sales__3B75D760");
        });

        modelBuilder.Entity<SalesMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesMas__3214EC07FE0D202E");

            entity.ToTable("SalesMaster");

            entity.HasIndex(e => e.BillNo, "UQ_SalesMaster_BillNo").IsUnique();

            entity.Property(e => e.BillDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BillNo).HasMaxLength(50);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasIndex(e => e.ItemId, "IX_StockTransactions_ItemId");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TransactionType).HasMaxLength(20);

            entity.HasOne(d => d.Item).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransactions_Items");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
