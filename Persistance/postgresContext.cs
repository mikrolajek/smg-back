using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WebApiRedirector.Models;

namespace WebApiRedirector.Persistance
{
    public partial class postgresContext : DbContext
    {
        public postgresContext()
        {
        }

        public postgresContext(DbContextOptions<postgresContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Code> Code { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Group> Group { get; set; }
        public virtual DbSet<Link> Link { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<PairTracker> PairTracker { get; set; }
        public virtual DbSet<PgBuffercache> PgBuffercache { get; set; }
        public virtual DbSet<PgStatStatements> PgStatStatements { get; set; }
        public virtual DbSet<Product> Product { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=samsung-db-redirector.postgres.database.azure.com;Database=postgres;Username=savoiradmin@samsung-db-redirector;Password='SavGroup!128';SSL Mode=Prefer;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pg_buffercache")
                .HasPostgresExtension("pg_stat_statements")
                .HasPostgresExtension("pgcrypto");

            modelBuilder.Entity<Code>(entity =>
            {
                entity.HasIndex(e => e.Uid)
                    .HasName("code_uid_key")
                    .IsUnique();
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.FromDate).HasDefaultValueSql("now()");

                entity.HasOne(d => d.IdCodeNavigation)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.IdCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("group_id_code_fkey");

                entity.HasOne(d => d.IdLinkNavigation)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.IdLink)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("group_id_link_fkey");

                entity.HasOne(d => d.IdLocationNavigation)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.IdLocation)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("group_id_location_fkey");

                entity.HasOne(d => d.IdPairTrackerNavigation)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.IdPairTracker)
                    .HasConstraintName("group_id_pair_tracker_fkey");

                entity.HasOne(d => d.IdProductNavigation)
                    .WithMany(p => p.Group)
                    .HasForeignKey(d => d.IdProduct)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("group_id_product_fkey");
            });

            modelBuilder.Entity<Link>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("nextval('url_id_seq'::regclass)");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("nextval('branch_id_seq'::regclass)");

                entity.HasOne(d => d.IdCompanyNavigation)
                    .WithMany(p => p.Location)
                    .HasForeignKey(d => d.IdCompany)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("branch_id_company_fkey");
            });

            modelBuilder.Entity<PairTracker>(entity =>
            {
                entity.HasOne(d => d.IdCode1Navigation)
                    .WithMany(p => p.PairTrackerIdCode1Navigation)
                    .HasForeignKey(d => d.IdCode1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pair_tracker_pair1_fkey");

                entity.HasOne(d => d.IdCode2Navigation)
                    .WithMany(p => p.PairTrackerIdCode2Navigation)
                    .HasForeignKey(d => d.IdCode2)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pair_tracker_pair2_fkey");
            });

            modelBuilder.Entity<PgBuffercache>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<PgStatStatements>(entity =>
            {
                entity.HasNoKey();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
