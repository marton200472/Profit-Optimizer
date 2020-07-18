using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProfitOptimizer
{
    static class Processor
    {
        static int[] StartSchedule = new int[6], EndSchedule = new int[6];
        static string[][] WorkStations = new string[6][];
        static int PreviousCompleteTime = 0;
        public static void Process(List<Order> orders)
        {
            WorkStations[0]= new string[] {"Vágó-1","Vágó-2" };
            WorkStations[1]= new string[] {"Hajlító-1","Hajlító-2" };
            WorkStations[2]= new string[] {"Hegesztő-1","Hegesztő-2" };
            WorkStations[3]= new string[] {"Tesztelő" };
            WorkStations[4]= new string[] {"Festő-1","Festő-2","Festő-3" };
            WorkStations[5]= new string[] {"Csomagoló-1","Csomagoló-2" };
            Program.SecondOutput = new List<string[]>();
            int ElapsedMinutes = 0;
            int profit = 0;
            int index = 0;
            
            ProductTypes PreviousProductType = ProductTypes.Error;
            
            
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
                
                switch (orders[0].ProductType)
                {
                    case ProductTypes.GYB:
                        StartSchedule[0] = ElapsedMinutes;
                        StartSchedule[1] = ElapsedMinutes + 5;
                        StartSchedule[2] = ElapsedMinutes + 15;
                        StartSchedule[3] = ElapsedMinutes + 23;
                        StartSchedule[4] = ElapsedMinutes + 28;
                        StartSchedule[5] = ElapsedMinutes + 40;
                        EndSchedule[0] = ElapsedMinutes+orders[0].TimeToFinishCutting;
                        EndSchedule[1] = EndSchedule[0] + 10;
                        EndSchedule[2] = EndSchedule[1] + 8;
                        EndSchedule[5] = orders[0].TimeToComplete;
                        EndSchedule[4] = EndSchedule[5] - 10;
                        EndSchedule[3] = EndSchedule[4] -12;
                        
                        
                        break;
                    case ProductTypes.SB:
                        StartSchedule[0] = ElapsedMinutes;
                        StartSchedule[1] = ElapsedMinutes + 6;
                        StartSchedule[2] = ElapsedMinutes + 15;
                        StartSchedule[3] = ElapsedMinutes + 10;
                        StartSchedule[4] = ElapsedMinutes + 5;
                        StartSchedule[5] = ElapsedMinutes + 15;
                        EndSchedule[0] = ElapsedMinutes + orders[0].TimeToFinishCutting;
                        EndSchedule[1] = EndSchedule[0] + 15;
                        EndSchedule[2] = EndSchedule[1] + 10;
                        EndSchedule[5] = orders[0].TimeToComplete;
                        EndSchedule[4] = EndSchedule[5] - 12;
                        EndSchedule[3] = EndSchedule[4] - 15;
                        
                        break;
                    case ProductTypes.FB:
                        StartSchedule[0] = ElapsedMinutes;
                        StartSchedule[1] = ElapsedMinutes + 8;
                        StartSchedule[2] = ElapsedMinutes + 16;
                        StartSchedule[3] = ElapsedMinutes + 12;
                        StartSchedule[4] = ElapsedMinutes + 5;
                        StartSchedule[5] = ElapsedMinutes + 20;
                        EndSchedule[0] = ElapsedMinutes + orders[0].TimeToFinishCutting;
                        EndSchedule[1] = EndSchedule[0] + 16;
                        EndSchedule[2] = EndSchedule[1] + 12;
                        EndSchedule[5] = orders[0].TimeToComplete;
                        EndSchedule[4] = EndSchedule[5] - 15;
                        EndSchedule[3] = EndSchedule[4] - 20;
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

                Program.SecondOutput.AddRange(GetScheduleData(orders[0].ProductType,orders[0].Identifier));
                
                index+=1;
                orders.RemoveAt(0);
                PreviousCompleteTime = ElapsedMinutes;
            }
            
            Logger.LogEntry("Az optimalizált rendelések feldolgozása megtörtént.");
            CsvWriter.WriteOrdersToCsv(Program.FirstOutput);
            CsvWriter.WriteWorkToCsv(Program.SecondOutput);
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
            return new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortDateString().Substring(6).RemoveWhitespace()+" " + new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortTimeString();
        }

        public static string ConvertToDateWithYear(int mins)
        {

            int[] dateTime = new int[] { 7, 20, 6, 0, 2020 };
            dateTime[1] += mins / 960;
            dateTime[2] += (mins % 960) / 60;
            dateTime[3] += (mins % 960) % 60;
            while (dateTime[1] > DateTime.DaysInMonth(dateTime[4], dateTime[0]))
            {
                dateTime[1] -= DateTime.DaysInMonth(dateTime[4], dateTime[0]);
                dateTime[0] += 1;
                if (dateTime[0] > 12)
                {
                    dateTime[0] = 1;
                    dateTime[4] += 1;
                }
            }
            return new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortDateString().RemoveWhitespace();
        }

        private static List<string[]> GetScheduleData(ProductTypes productType,string Identifier)
        {
            List<string[]> output = new List<string[]>();
            int daysElapsed = (EndSchedule[0] - StartSchedule[0]) / 960;
            if (PreviousCompleteTime % 960 + (EndSchedule[0] - StartSchedule[0]) % 960 >= 960)
            {
                daysElapsed++;
            }
            //az i a workstations elemét jelzi
            for (int i = 0; i < 6; i++)
            {
                //counter: workstations[i] elemein végigmegy
                int counter = 0;
                
                
                while (counter < WorkStations[i].Length)
                {
                    
                    for (int j = 0; j <= daysElapsed; j++)
                    {
                        
                        if (WorkStations[i][counter] == "Vágó-1")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(StartSchedule[i] + j * 960);
                            
                            output.Last()[1] = WorkStations[i][counter];
                            if (j!=0&&j!=daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j==0&&j==daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(EndSchedule[i]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(StartSchedule[i]).Split(' ')[1];
                            }
                            else if(j==0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(StartSchedule[i]).Split(' ')[1];
                            }
                            else if (j==daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(EndSchedule[i]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }
                            
                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Vágó-2"&&j==0)
                        {
                            output.Add(new string[5]);
                            int AdditionalTime = productType == ProductTypes.GYB ? 5 : (productType == ProductTypes.SB ? 6 : 8);
                            output.Last()[3] = ConvertToDate(StartSchedule[i] + AdditionalTime).Split(' ')[1];
                            output.Last()[0] = ConvertToDateWithYear(StartSchedule[i] + j * 960);
                            output.Last()[1] = WorkStations[i][counter];
                            output.Last()[2] = ConvertToDate(StartSchedule[i]).Split(' ')[1];
                            output.Last()[4] = Identifier;
                        }
                        
                        
                    }

                    
                    counter++;
                }
            }
            
            return output;
        }
    }
}
