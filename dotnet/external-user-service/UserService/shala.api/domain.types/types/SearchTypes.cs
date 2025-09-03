namespace shala.api.domain.types;

public enum SortOrder
{
    Ascending,
    Descending
}

public class BaseSearchFilters
{
    public int PageIndex { get; set; } = 0;
    public int ItemsPerPage { get; set; } = 25;
    public string? OrderBy { get; set; }
    public string? Sort { get; set; } = "desc";
    public SortOrder? Order
    {
        get
        {
            return this.Sort == "desc" ? SortOrder.Descending : SortOrder.Ascending;
        }
        set
        {
            this.Sort = value == SortOrder.Descending ? "desc" : "asc";
        }
    }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
}

public class SearchResults<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int PageIndex { get; set; } = 0;
    public int ItemsPerPage { get; set; } = 25;
    public int RetrievedCount { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
