
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Resources;

namespace ProfitOptimizer
{
    public static class SequenceFinder
    {
        public static Order[] requests;
        public static void Optimize()
        {
            //betölti az adatokat a fejléc kivételével
            Console.WriteLine("A forrásfájl kiválaszttásához használd a megjelenő ablakot.");
            System.Threading.Thread.Sleep(1000);
            try
            {
                requests = CsvLoader.GetInputFile();
                
            }
            catch (Exception)
            {
                Logger.LogEntry("Hiba történt az adatok betöltése közben, ellenőrizd a forrásfájlt! Leggyakoribb ok a dátum helytelen formátumban való használata.");
                Program.Error();
            }
            try
            {
                Sort();
                Logger.LogEntry("Az adatok optimalizálása sikeresen megtörtént.");
            }
            catch (Exception)
            {

                Logger.LogEntry("Hiba történt az adatok optimalizálása közben!");
                Program.Error();

            }
            Processor.Process(requests.ToList());
            
            
            

        }
        public static void Optimize(string path)
        {
            //betölti az adatokat a fejléc kivételével
            Console.WriteLine("Forrásfájl kijelölve.");
            try
            {
                
                requests = CsvLoader.GetSplitter(path);
                
            }
            catch (Exception)
            {
                Logger.LogEntry("Hiba történt az adatok betöltése közben, ellenőrizd a forrásfájlt! Lehetséges okok: hibás formátum, vagy a fájlt egy másik folyamat használja!");
                Program.Error();
            }
            try
            {
                Sort();
                Logger.LogEntry("Az összes rendelés optimalizálása sikeresen megtörtént.");
            }
            catch (Exception)
            {

                Logger.LogEntry("Hiba történt az adatok optimalizálása közben!");
                Program.Error();

            }
            Processor.Process(requests.ToList());




        }



        private static void Sort()
        {
            
            //List<Order> SortedOrders = new List<Order>();
            
            


            for (int i = 0; i < requests.Length; i++)
            {
                if (requests[i].ProductType == ProductTypes.GYB)
                {
                    requests[i].TimeToFinishCutting = (requests[i].Pieces - 1) * 5;
                    requests[i].TimeToComplete = requests[i].TimeToFinishCutting + 50;
                }
                else if (requests[i].ProductType == ProductTypes.SB)
                {
                    requests[i].TimeToFinishCutting = (int)Math.Ceiling(7.5*(requests[i].Pieces-2)+6);
                    requests[i].TimeToComplete = 63 + ((requests[i].Pieces - 1) / 2 * 15) + (requests[i].Pieces % 2 == 0 ? 5 : 0);
                }
                else if (requests[i].ProductType == ProductTypes.FB)
                {
                    requests[i].TimeToFinishCutting = (requests[i].Pieces - 1) * 8;
                    requests[i].TimeToComplete = 76+((requests[i].Pieces-1)/2*16)+(requests[i].Pieces%2==0?5:0);
                }

            }
            Logger.LogEntry("A gyártási idő kiszámítása sikeresen megtörtént.");
            for (int i = 0; i < requests.Length; i++)
            {
                requests[i].Priority = ((requests[i].TimeToComplete/100)*requests[i].TimeLeft*100) / ((requests[i].IncomePerPiece/100)*requests[i].DelayPenalty / 1000);
            }
            for (int i = 0; i < requests.Length-1; i++)
            {
                for (int j = i+1; j < requests.Length; j++)
                {
                    if (requests[i].Priority>requests[j].Priority)
                    {
                        var tmp = requests[i];
                        requests[i] = requests[j];
                        requests[j] = tmp;
                    }
                }
                Logger.LogEntry("A(z) "+ requests[i].Identifier + " azonosítójú rendelés optimalizálása és gyártási várólistához adása megtörtént." );
                
            }
            foreach (var item in requests)
            {
                Console.WriteLine(item.Priority);
            }


        }

    }
}
