using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vendas
{
    internal class Program
    {
        private static readonly string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string docPathIn;
        private static string docPathProcessed;
        private static string docPathOut;
        private static int countSalesman;
        private static int countClient;
        private static decimal topSale;
        private static ConcurrentDictionary<string, decimal> dictionarySalesmanSales = new ConcurrentDictionary<string, decimal>();

        public static void Main(string[] args)
        {
            string data = Directory.CreateDirectory(Path.Combine(homePath, @"data")).FullName;
            docPathIn = Directory.CreateDirectory(Path.Combine(data, @"in")).FullName;
            docPathProcessed = Directory.CreateDirectory(Path.Combine(data, @"processed")).FullName;
            docPathOut = Directory.CreateDirectory(Path.Combine(data, @"out")).FullName;
            while (true)
            {
                ProcessRead();
            }
        }

        private static void ProcessRead()
        {
            List<string> allLines = new List<string>();

            IEnumerable<string> fileEntries = Directory.EnumerateFiles(docPathIn);
            foreach (string fname in fileEntries)

            {
                string nameFile = Path.GetFileName(fname);
                try
                {
                    allLines = new List<string>();
                    using (StreamReader sr = File.OpenText(fname))
                    {
                        while (!sr.EndOfStream)
                        {
                            allLines.Add(sr.ReadLine());
                        }
                    } //Fechando arquivo, pois não é mais necessário
                    Parallel.ForEach(allLines, line =>
                    {
                        try
                        {
                            dynamic parsedLine = Parser.ParseString(line);
                            if (parsedLine != null)
                            {
                                Processor(parsedLine);
                            }
                        }
                        catch (FormatException f)
                        {
                            Console.WriteLine(f);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("Insufficient memory");
                }
                finally
                {
                    if (allLines.Count > 0)
                    {
                        allLines.Clear();
                        allLines = null;

                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPathOut, "output_" + nameFile)))
                        {
                            outputFile.WriteAsync(string.Format(@"Client count: {0}; Salesman count: {1}; Top sale: {2}; Worst salesman: {3}.",
                                countClient, countSalesman, topSale,
                                dictionarySalesmanSales.Aggregate((l, r) => l.Value < r.Value ? l : r).Key));
                        }
                        File.Move(fname, Path.Combine(docPathProcessed, nameFile));
                    }
                }
                GC.Collect();
            }
        }

        private static void Processor(dynamic dynamic)
        {
            if (dynamic.GetType() == typeof(Salesman))
            {
                countSalesman++;
                Salesman salesman = dynamic;
                dictionarySalesmanSales.TryAdd(salesman.Name, 0);
            }
            else if (dynamic.GetType() == typeof(Client))
            {
                countClient++;
            }
            else if (dynamic.GetType() == typeof(Sale))
            {
                Sale sale = dynamic;
                if (sale.TotalSale > topSale)
                {
                    topSale = sale.TotalSale;
                }
                if (dictionarySalesmanSales.ContainsKey(sale.SalesmanName))
                {
                    dictionarySalesmanSales[sale.SalesmanName] += sale.TotalSale;
                }
            }
        }
    }
}