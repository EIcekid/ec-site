using System.ComponentModel.DataAnnotations.Schema;

namespace EcSite.Api.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string? Color { get; set; }
    public string? Size { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public int Stock { get; set; }

    [NotMapped]
    public string Label
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Color)) parts.Add($"カラー：{Color}");
            if (!string.IsNullOrEmpty(Size)) parts.Add($"サイズ：{Size}");
            return string.Join("／", parts);
        }
    }
}
