using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using PeriodTracker.Entities.Framework;
using PeriodTracker.Entities.PeriodTracker;

namespace PeriodTracker.Entities
{
    public class DataContext : DbContext
    {
        public DataContext() : base()
        {
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is CycleProfile cycleProfile)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (cycleProfile.CreatedAtUtc == default)
                        {
                            cycleProfile.CreatedAtUtc = utcNow;
                        }

                        cycleProfile.UpdatedAtUtc = utcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(CycleProfile.CreatedAtUtc)).IsModified = false;
                        cycleProfile.UpdatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is Cycle cycle)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (cycle.CreatedAtUtc == default)
                        {
                            cycle.CreatedAtUtc = utcNow;
                        }

                        cycle.UpdatedAtUtc = utcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(Cycle.CreatedAtUtc)).IsModified = false;
                        cycle.UpdatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is DailyLog dailyLog)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (dailyLog.CreatedAtUtc == default)
                        {
                            dailyLog.CreatedAtUtc = utcNow;
                        }

                        dailyLog.UpdatedAtUtc = utcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(DailyLog.CreatedAtUtc)).IsModified = false;
                        dailyLog.UpdatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is DailyMetric dailyMetric)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (dailyMetric.CreatedAtUtc == default)
                        {
                            dailyMetric.CreatedAtUtc = utcNow;
                        }

                        dailyMetric.UpdatedAtUtc = utcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(DailyMetric.CreatedAtUtc)).IsModified = false;
                        dailyMetric.UpdatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is Symptom symptom)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (symptom.CreatedAtUtc == default)
                        {
                            symptom.CreatedAtUtc = utcNow;
                        }

                        symptom.UpdatedAtUtc = utcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(Symptom.CreatedAtUtc)).IsModified = false;
                        symptom.UpdatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is DailyLogSymptom dailyLogSymptom && entry.State == EntityState.Added)
                {
                    if (dailyLogSymptom.CreatedAtUtc == default)
                    {
                        dailyLogSymptom.CreatedAtUtc = utcNow;
                    }
                }

                if (entry.Entity is DailyLogSymptom && entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(DailyLogSymptom.CreatedAtUtc)).IsModified = false;
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DataContext"), cfg => cfg.UseCompatibilityLevel(120));
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CycleProfile>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.TypicalCycleLengthDays).HasDefaultValue((short)28);
                e.Property(x => x.TypicalPeriodLengthDays).HasDefaultValue((short)5);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_PT_CycleProfiles_TypicalCycleLengthDays",
                        "[TypicalCycleLengthDays] >= 15 AND [TypicalCycleLengthDays] <= 60");
                    t.HasCheckConstraint(
                        "CK_PT_CycleProfiles_TypicalPeriodLengthDays",
                        "[TypicalPeriodLengthDays] >= 1 AND [TypicalPeriodLengthDays] <= 15");
                });
            });

            modelBuilder.Entity<Cycle>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.Source).HasDefaultValue("Manual");
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                // Enforce a single open cycle per user.
                e.HasIndex(x => x.UserId)
                    .HasDatabaseName("IX_PT_Cycles_UserId_OpenCycle")
                    .IsUnique()
                    .HasFilter("[EndDate] IS NULL");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_PT_Cycles_EndDate_After_StartDate",
                        "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                });
            });

            modelBuilder.Entity<DailyLog>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<DailyMetric>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_PT_DailyMetrics_MetricType_Allowed",
                        "[MetricType] IN ('Weight', 'Temperature', 'Water')");
                    t.HasCheckConstraint(
                        "CK_PT_DailyMetrics_DecimalValue_Positive",
                        "[DecimalValue] > 0");
                });
            });

            modelBuilder.Entity<Symptom>(e =>
            {
                e.Property(x => x.SortOrder).HasDefaultValue((short)0);
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<DailyLogSymptom>(e =>
            {
                e.HasOne(x => x.DailyLog)
                    .WithMany()
                    .HasForeignKey(x => x.DailyLogId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Symptom)
                    .WithMany()
                    .HasForeignKey(x => x.SymptomId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_PT_DailyLogSymptoms_Severity_Valid",
                        "[Severity] IS NULL OR ([Severity] >= 1 AND [Severity] <= 5)");
                });
            });
        }

        #region
        public DbSet<Log> Logs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<CycleProfile> CycleProfiles { get; set; }
        public DbSet<Cycle> Cycles { get; set; }
        public DbSet<DailyLog> DailyLogs { get; set; }
        public DbSet<DailyMetric> DailyMetrics { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<DailyLogSymptom> DailyLogSymptoms { get; set; }
        #endregion
    }
}
