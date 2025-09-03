namespace shala.api.domain.types;

public class FileResource
{
    public Guid Id { get; set; }
    public Guid? OwnerUserId { get; set; } = Guid.Empty;
    public Guid? TenantId { get; set; } = Guid.Empty;
    public string FileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public bool IsPublic { get; set; } = false;
    public long FileSize { get; set; } = 0;
    public string? MimeType { get; set; } = null!;
    public string? FileExtension { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class FileResourceCreateModel
{
    public Guid OwnerUserId { get; set; } = Guid.Empty;
    public Guid? TenantId { get; set; } = Guid.Empty;
    public string FileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public bool IsPublic { get; set; } = false;
    public long FileSize { get; set; } = 0;
    public string? MimeType { get; set; } = null!;
    public string? FileExtension { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FileResourceSearchFilters : BaseSearchFilters
{
    public string? FileName { get; set; } = null!;
    public Guid? OwnerUserId { get; set; } = Guid.Empty;
    public Guid? TenantId { get; set; } = Guid.Empty;
}
