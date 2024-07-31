using Microsoft.Net.Http.Headers;

namespace HPlusSport.API;

public class QueryParameters
{
    const int _maxSize = 100;   

    private int _size = 50;

    public int Page { get; set; } = 1;
    public int Size 
    { 
        get { return _size;}
        set {
                _size = Math.Min(_maxSize, value);
            }
    }

    public string SortBy { get; set; } = "Id";
    public string _sortOrder { get; set; } = "asc";
    public string SortOrder
    {
        get { return _sortOrder;}
    // Usar um allow list no set, porque só tem 2 valores válidos (asc e desc)
    // Vamos checar se tem um do dois valores
        set {
                // Somente nestes 2 casos eu vou definir o valor de set
                if(value == "asc" || value == "desc")
                {
                    _sortOrder = value;
                }
            }
    }
}
