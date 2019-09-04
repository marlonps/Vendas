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
        private static readonly string docPathIn =  @"C:\Users\marlon.silva\Documents\data\in";

        private static readonly string docPathOut = @"C:\Users\marlon.silva\Documents\data\out";
        private static int countSalesman;
        private static int countClient;
        private static decimal topSale;
        private static  ConcurrentDictionary<string, decimal> dictionarySalesmanSales = new ConcurrentDictionary<string, decimal>();

        public static void Main(string[] args)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher(docPathIn))
            {
                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                watcher.Filter = "";
                // Add event handlers.
                watcher.Created += OnChanged;

                watcher.EnableRaisingEvents = true;
                //Console.WriteLine("Press 'q' to quit the sample.");
                //while (Console.Read() != 'q') ;
            }
            while (true)
            {
                ProcessRead();
            }

        }

        private static async void OnChanged(object sender, FileSystemEventArgs e)
        {
            await ProcessRead();
        }

        private static async Task ProcessRead()
        {
            //se soubermos o tamanho maximo de linhas do arquivo - ou estipularmos um limite
            //podemos usar um Array em vez de List, o que aumenta a performance, devido à alocação fixa de memória
            //memcached, sqlite, redis -> memoria
            //filebeats -> logstash -> output
            List<string> allLines = new List<string>();
            //int MAX = 0;
            await Task.Run(() =>
            {
                IEnumerable<string> fileEntries = Directory.EnumerateFiles(docPathIn);
                foreach (string fname in fileEntries)
                {
                    try
                    {
                        allLines = new List<string>();
                        using (StreamReader sr = File.OpenText(fname))
                        {
                            while (!sr.EndOfStream)
                            {
                                allLines.Add(sr.ReadLine());
                            }
                        } //CLOSE THE FILE because we are now DONE with it.
                        Parallel.ForEach(allLines, line =>
                        {
                            Processor(Parser.ParseString(line));
                        });
                    }
                    catch (OutOfMemoryException)
                    {
                        Console.WriteLine("Not enough memory. Couldn't perform this test.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("EXCEPTION. Couldn't perform this test.");
                    }
                    finally
                    {
                        if (allLines != null)
                        {
                            allLines.Clear();
                            allLines = null;
                        }
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPathOut, "WriteTextAsync.txt")))
                        {
                            outputFile.WriteAsync(string.Format(@"Client count: {0}, Salesman count: {1}, Top sale: {2},
                                Worse salesman: {3}", countClient, countSalesman, topSale, 
                                dictionarySalesmanSales.Min().Key));
                        }
                        // File.Move(arq, nomeErro);
                    }
                    GC.Collect();
                }
            });
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