namespace GeneralizeQueries.Core.DTOs.AuditLog;

public class AuditLogViewModel
{
    public Guid Id { get; set; }
    public string? ServiceId { get; set; }
    public string? ItemId { get; set; }
    public string? Action { get; set; }
    public Guid UserId { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime LogTime { get; set; }
}

public class AuditLogDetailViewModel
{
    public Guid Id { get; set; }
    public string? ServiceId { get; set; }
    public string? ItemId { get; set; }
    public string? Action { get; set; }
    public Guid UserId { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? PayLoad { get; set; }
    public DateTime LogTime { get; set; }
}