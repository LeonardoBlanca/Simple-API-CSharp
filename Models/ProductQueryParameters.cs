namespace HPlusSport.API;

public class ProductQueryParameters : QueryParameters
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
