using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas
{
    public static class Parser
    {
        public static dynamic ParseString(string line)
        {
            string[] parsedString = line.Split('ç');
            try
            {
                switch (int.TryParse(parsedString[0], out int identifier) ? identifier :
                    throw new FormatException(string.Format("Class identifier is not an integer: {0}", parsedString[0])))
                {
                    case (1):
                        return new Salesman
                        {
                            Cpf = parsedString[1],
                            Name = parsedString[2],
                            Salary = decimal.TryParse(parsedString[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal salary) ? salary : 
                            throw new FormatException(string.Format("Salary is not a decimal: {0}", parsedString[3]))
                        };

                    case (2):
                        return new Client
                        {
                            Cnpj = parsedString[1],
                            Name = parsedString[2],
                            BusinessArea = parsedString[3]
                        };

                    case (3):
                        List<ItemSale> itemSale = new List<ItemSale>();
                        try
                        {
                            itemSale = BuildListItems(parsedString);
                        }
                        catch (FormatException fe)
                        {
                            throw fe;
                        }
                        return new Sale
                        {
                            SaleId = int.TryParse(parsedString[1], out int id) ? id : 
                            throw new FormatException(string.Format("SaleId is not an integer: {0}", parsedString[1])),
                            ItemSale = itemSale,
                            SalesmanName = parsedString[3]
                        };

                    default:
                        throw new Exception(string.Format("Unrecognized Class: {0}", identifier));
                }
            }catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static List<ItemSale> BuildListItems(string[] parsedString)
        {
            List<string> listItems = parsedString[2].Split(',').ToList();
            ConcurrentBag<ItemSale> itemSale = new ConcurrentBag<ItemSale>();
            Parallel.ForEach(listItems, item =>
            {
                string formattedString = item.Replace("[", "").Replace("]", "");
                string[] itemParsed = formattedString.Split('-');
                try
                {
                    itemSale.Add(new ItemSale
                    {
                        IdItem = int.TryParse(itemParsed[0], out int idItem) ? idItem :
                            throw new FormatException(string.Format("IdItem is not an integer: {0}", parsedString[0])),
                        Quantity = int.TryParse(itemParsed[1], out int quantity) ? quantity :
                            throw new FormatException(string.Format("Quantity is not an integer: {0}", parsedString[1])),
                        Price = decimal.TryParse(itemParsed[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) ? price :
                            throw new FormatException(string.Format("Price is not a decimal: {0}", parsedString[2])),
                    });
                }
                catch (FormatException fe)
                {
                    Console.WriteLine(fe);
                }
            });
            return itemSale.ToList();
        }
    }
}