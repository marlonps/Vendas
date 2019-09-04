using System.Collections.Generic;
using System.Linq;

namespace Vendas
{
    public class Sale
    {
        public int SaleId { get; set; }
        public List<ItemSale> ItemSale { get; set; }
        public string SalesmanName { get; set; }

        private decimal totalSale;
        public decimal TotalSale
        {
            get => totalSale;
            set => totalSale = ItemSale.Sum(it => it.Price * it.Quantity);
        }
    }
}