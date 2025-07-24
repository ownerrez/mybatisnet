using System;

namespace ORBatis.Test.Common.Models;

public class Review
{
    public decimal? StarsOutOf5 { get; set; }
    public bool IsAutoReviewManuallyCanceled { get; set; }
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int CreatedUserId { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public int UpdatedUserId { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public int DeletedUserId { get; set; }
}