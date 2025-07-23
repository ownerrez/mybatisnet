using System;

namespace ORBatis.Test.Common.Models;

public class Holiday
{
    public Holiday()
    {
        AllowAllProperties = true;
        Active = true;
    }

    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public bool AllowAllProperties { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int CreatedUserId { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public int UpdatedUserId { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public int DeletedUserId { get; set; }
}