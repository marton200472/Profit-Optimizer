using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitOptimizer
{
    static class Processor
    {
        public static void Process(List<Order> orders)
        {
            int ElapsedMinutes = 0;
            int profit = 0;
            int index = 0;
            int PreviousCompleteTime = 0;
            ProductTypes PreviousProductType = ProductTypes.Error;
            //DateTime StartTime = new DateTime(2020,7,20,6,0,0);
            
            Program.FirstOutput = new string[orders.Count()][];

            while (orders.Count>0)
            {
                var profitonthis = 0;
                
                switch (orders[0].ProductType)
                {
                    case ProductTypes.GYB:
                        switch (PreviousProductType)
                        {
                            case ProductTypes.SB: ElapsedMinutes += 13;
                                break;
                            case ProductTypes.FB: ElapsedMinutes += 26;
                                break;
                        }
                        break;
                    case ProductTypes.SB: if (PreviousProductType == ProductTypes.FB) ElapsedMinutes += 13;
                        break;
                }
                var CompleteTime = ElapsedMinutes + orders[0].TimeToComplete;
                ElapsedMinutes += orders[0].TimeToFinishCutting;
                
                profitonthis += orders[0].Pieces * orders[0].IncomePerPiece;

                if (CompleteTime>orders[0].TimeLeft)
                {
                    var elapsedDays = (CompleteTime - orders[0].TimeLeft) / 960;
                    if ((CompleteTime - orders[0].TimeLeft) % 960>0)
                    {
                        elapsedDays++;
                    }
                    profitonthis -=elapsedDays*orders[0].DelayPenalty;

                }
                profit += profitonthis;
               

                Program.FirstOutput[index] = new string[6];
                Program.FirstOutput[index][0] = orders[0].Identifier;
                Program.FirstOutput[index][1] = profitonthis + " Ft";
                Program.FirstOutput[index][2] = orders[0].Pieces * orders[0].IncomePerPiece - profitonthis + " Ft";
                Program.FirstOutput[index][3] = ConvertToDate(PreviousCompleteTime);
                Program.FirstOutput[index][4] = ConvertToDate(CompleteTime);
                Program.FirstOutput[index][5] = ConvertToDate(orders[0].TimeLeft);
                index+=1;
                orders.RemoveAt(0);
                PreviousCompleteTime = ElapsedMinutes;
            }
            Logger.LogEntry("Az optimalizált rendelések feldolgozása megtörtént.");
            CsvWriter.WriteOrdersToCsv(Program.FirstOutput);
            Console.WriteLine("Az eltelt idő "+ ElapsedMinutes / 960 + " nap.");
            Console.WriteLine("A profit {1} HUF",profit,profit);
        }

        public static string ConvertToDate(int mins)
        {
            
            int[] dateTime = new int[] {7,20,6,0,2020 };
            dateTime[1] += mins / 960;
            dateTime[2] += (mins % 960) / 60;
            dateTime[3] += (mins % 960) % 60;
            while (dateTime[1]>DateTime.DaysInMonth(dateTime[4],dateTime[0]))
            {
                dateTime[1] -= DateTime.DaysInMonth(dateTime[4], dateTime[0]);
                dateTime[0] += 1;
                if (dateTime[0]>12)
                {
                    dateTime[0] = 1;
                    dateTime[4] += 1;
                }
            }
            return new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortDateString().Substring(6)+" " + new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortTimeString();
        }
    }
}
