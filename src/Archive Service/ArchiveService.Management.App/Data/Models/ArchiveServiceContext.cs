using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace ArchiveService.Management.App.Data.Models;

public partial class ArchiveServiceContext : DbContext
{
    readonly DateTimeOffset? tokenexpiry = null;
    readonly Azure.Core.AccessToken token;
    public ArchiveServiceContext(DbContextOptions<ArchiveServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchiveServiceSetting> ArchiveServiceSettings { get; set; }

    public virtual DbSet<ArchiveTableSetting> ArchiveTableSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchiveServiceSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CPK_ArchiveSetting_Id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
