using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArchiveService.Management.App.Data.Models;

public partial class ArchiveServiceSetting
{
    public bool? HistoricalArchive { get; set; }

    public bool? ChangeLogEnabled { get; set; }

    public bool? ArchiveEnabled { get; set; }

    [StringLength(255)]
    public string? DataverseUrl { get; set; }

    [StringLength(10)]
    public string? SchemaName { get; set; }

    public Guid? ServiceEndPointId { get; set; }

    [StringLength(255)]
    public string? PublishServiceUrl { get; set; }

    [StringLength(255)]
    public string? ArchiveDataverseServiceUrl { get; set; }

    [Key]
    public int Id { get; set; }
}
