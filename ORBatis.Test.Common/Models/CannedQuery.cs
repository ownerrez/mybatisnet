using System;

namespace ORBatis.Test.Common.Models;

public class CannedQuery
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string Name { get; set; }

    public string Description { get; set; }

    public string Sql { get; set; }

    public int? LimitGrid { get; set; }

    public int? LimitExport { get; set; }

    public int? Timeout { get; set; }

    public CannedQueryParameter[] Parameters { get; set; }

    public EntityType[] EntityTypes { get; set; }

    public string RelatedItems { get; set; }

    public string ExportFilename { get; set; }
    
    public DateTime CreatedUtc { get; set; }
    public int CreatedUserId { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public int UpdatedUserId { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public int DeletedUserId { get; set; }
}

public class CannedQueryParameter
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public ParameterType Type { get; set; }

    public bool IsOptional { get; set; }

    public string HelpInfo { get; set; }
}

public enum ParameterType
{
    DateTime,
    String,
    Integer,
    Decimal,
    Boolean,
    Guid
}