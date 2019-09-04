using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas
{
    public static class Parser
    {
        public static dynamic ParseString(string line)
        {
            string[] parsedString = line.Split('ç');
            switch (int.TryParse(parsedString[0], out int identifier) ? identifier : throw new FormatException(""))
            {
                case (1):
                    return new Salesman
                    {
                        Cpf = int.TryParse(parsedString[1], out int cpf) ? cpf : throw new FormatException(""),
                        Name = parsedString[2],
                        Salary = decimal.TryParse(parsedString[3], out decimal salary) ? salary : throw new FormatException("")
                    };

                case (2):
                    return new Client
                    {
                        Cnpj = long.TryParse(parsedString[1], out long cnpj) ? cnpj : throw new FormatException(""),
                        Name = parsedString[2],
                        BusinessArea = parsedString[3]
                    };

                case (3):
                    List<ItemSale> itemSale = BuildListItems(parsedString);
                    return new Sale
                    {
                        SaleId = int.TryParse(parsedString[1], out int id) ? id : throw new FormatException(""),
                        ItemSale = itemSale,
                        SalesmanName = parsedString[3]
                    };

                default:
                    throw new Exception("");
            }
        }

        private static List<ItemSale> BuildListItems(string[] parsedString)
        {
            List<string> listItems = parsedString[2].Split(',').ToList();
            ConcurrentBag<ItemSale> itemSale = new ConcurrentBag<ItemSale>();
            Parallel.ForEach(listItems, item =>
            {
                string formattedString = item.Replace("[", "").Replace("]",",");
                string[] itemParsed = formattedString.Split('-');
        
                itemSale.Add(new ItemSale
                {
                    IdItem = int.TryParse(itemParsed[0], out int idItem) ? idItem : throw new FormatException(),
                    Quantity = int.TryParse(itemParsed[1], out int quantity) ? quantity : throw new FormatException(),
                    Price = decimal.TryParse(itemParsed[2], out decimal price) ? price : throw new FormatException(),
                });
            });
            return itemSale.ToList();
        }
    }
}