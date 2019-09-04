using System.Collections.Generic;
using System.Linq;

namespace Vendas
{
    public class Sale
    {
        public int SaleId { get; set; }
        public List<ItemSale> ItemSale { get; set; }
        public string SalesmanName { get; set; }

        public decimal TotalSale
        {
            get => ItemSale.Sum(it => it.Price * it.Quantity);
        }
    }
}