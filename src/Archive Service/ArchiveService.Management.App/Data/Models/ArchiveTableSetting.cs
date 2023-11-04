using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArchiveService.Management.App.Data.Models;

[Index("LogicalName", Name = "UIX_LogicalName", IsUnique = true)]
public partial class ArchiveTableSetting
{
    [Key]
    public int Id { get; set; }

    [StringLength(510)]
    public string? DisplayName { get; set; }

    [StringLength(510)]
    public string? LogicalName { get; set; }

    public bool? ArchiveEnabled { get; set; }

    public bool? ChangeLogEnabled { get; set; }

    [StringLength(1000)]
    public string? LastChangeTrackingToken { get; set; }

    public Guid? ChangeLogPluginStepId { get; set; }
}
