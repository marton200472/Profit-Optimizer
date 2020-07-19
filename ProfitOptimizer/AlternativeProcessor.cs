using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitOptimizer
{
    public static class AlternativeProcessor
    {
        static double TimeInMinutes = 0;
        static double[][] StartSchedule=new double[6][], EndSchedule = new double[6][];
        public static string[][] WorkStations = new string[6][];
        public static void Process(Order[] orders)
        {
            WorkStations[0] = new string[] { "Vágó-1", "Vágó-2" };
            WorkStations[1] = new string[] { "Hajlító-1", "Hajlító-2" };
            WorkStations[2] = new string[] { "Hegesztő-1", "Hegesztő-2" };
            WorkStations[3] = new string[] { "Tesztelő" };
            WorkStations[4] = new string[] { "Festő-1", "Festő-2","Festő-3" };
            WorkStations[5] = new string[] { "Csomagoló-1", "Csomagoló-2" };
            Program.FirstOutput = new string[orders.Length][];
            Program.SecondOutput=new List<string[]>();
            ProductTypes PreviousProductType = ProductTypes.Error;
            for (int i = 0; i < orders.Length; i++)
            {
                if (TimeInMinutes!=0)
                {
                    TimeInMinutes -= 50;
                }
                double StartTime = TimeInMinutes;
                
                
                
                PreviousProductType = orders[0].ProductType;
                ProcessOrder(orders[i]);
                int profitonthis;
                if (TimeInMinutes>orders[0].TimeLeft)
                {
                    profitonthis = orders[i].Pieces * orders[i].IncomePerPiece - ((((int)Math.Ceiling(TimeInMinutes) - orders[0].TimeLeft) / 960) * orders[i].DelayPenalty);
                }
                else
                {
                    profitonthis = orders[i].Pieces * orders[i].IncomePerPiece;
                }
                
                Program.FirstOutput[i] = new string[6];
                Program.FirstOutput[i][0] = orders[i].Identifier;
                Program.FirstOutput[i][1] = profitonthis + " Ft";
                Program.FirstOutput[i][2] = orders[i].Pieces * orders[i].IncomePerPiece - profitonthis + " Ft";
                Program.FirstOutput[i][3] = ConvertToDate((int)Math.Ceiling(StartTime));
                Program.FirstOutput[i][4] = ConvertToDate((int)Math.Ceiling(TimeInMinutes));
                Program.FirstOutput[i][5] = ConvertToDate(orders[i].TimeLeft);

                Program.SecondOutput.AddRange(GetScheduleData(orders[i].Pieces,orders[i].Identifier));
                
            }
            Logger.LogEntry("Az optimalizált rendelések feldolgozása megtörtént.");
            CsvWriter.WriteOrdersToCsv(Program.FirstOutput);
            CsvWriter.WriteWorkToCsv(Program.SecondOutput);
        }

        private static void ProcessOrder(Order order)
        {
            double[] ProcessTimes;
            int[] Pcs = new int[] {2,2,2,1,3,3 };
            int[] Working = new int[] {0,0,0,0,0,0 };
            int[] Queue = new int[6];
            double[][] FinishTimes = new double[6][];
            
            switch (order.ProductType)
            {
                case ProductTypes.GYB:
                    ProcessTimes = new double[] {5,10,8,5,12,10 };
                    break;
                case ProductTypes.SB:
                    ProcessTimes = new double[] { 7.5, 15, 10, 5, 15, 12 };
                    break;
                case ProductTypes.FB:
                    ProcessTimes = new double[] {8,16,12,5,20,15};
                    break;
                default:
                    ProcessTimes = new double[] { 5, 10, 8, 5, 12, 10 };
                    break;
            }
            Queue[0] = order.Pieces;
            
            for (int i = 0; i < 6; i++)
            {
                StartSchedule[i] = new double[Pcs[i]];
                EndSchedule[i] = new double[Pcs[i]];
                FinishTimes[i] = new double[Pcs[i]];
                

                for (int j = 0; j < Pcs[i]; j++)
                {
                    StartSchedule[i][j] = -1;
                    EndSchedule[i][j] = 0;
                    
                }
            }



            bool finished = false;
            while (!finished)
            {
                if (StartSchedule[0][0] == -1)
                {
                    Queue[0] -= 2;
                    Working[0] = 2;
                    StartSchedule[0][0] = TimeInMinutes;
                    FinishTimes[0][0] = TimeInMinutes + ProcessTimes[0];
                    StartSchedule[0][1] = TimeInMinutes;
                    EndSchedule[0][1] = TimeInMinutes + ProcessTimes[0];
                    
                }
                else if (TimeInMinutes == FinishTimes[0][0])
                {
                    
                    Queue[1] += Working[0];
                    Working[0] = 0;
                    if (Queue[0] > 0)
                    {
                        Queue[0]--;
                        Working[0] = 1;
                        FinishTimes[0][0] = TimeInMinutes + ProcessTimes[0];
                    }
                    else
                    {
                        EndSchedule[0][0] = TimeInMinutes;
                    }

                }
                for (int j = 1; j < 6; j++)
                {
                    for (int i = 0; i < Pcs[j]; i++)
                    {
                        bool continuework = false;
                        for (int k = 0; k < j; k++)
                        {
                           
                            if (Queue[k] > 0)
                            {
                                continuework = true;
                                break;
                            }
                        }

                        if (Queue[j] > 0 && StartSchedule[j][i] == -1)
                        {
                            StartSchedule[j][i] = TimeInMinutes;

                            Queue[j] -= 1;
                            Working[j] += 1;
                            FinishTimes[j][i] = TimeInMinutes + ProcessTimes[j];
                            
                        }
                        else if (Queue[j]>0&& TimeInMinutes >= FinishTimes[j][i])
                        {
                            Working[j] -= 1;
                            if (j != 5)
                            {
                                Queue[j + 1] += 1;
                            }
                            Queue[j] -= 1;
                            Working[j] += 1;
                            FinishTimes[j][i] = TimeInMinutes + ProcessTimes[1];
                        }
                        else if (!continuework)
                        {
                            Working[j] -= 1;
                            EndSchedule[j][i] = TimeInMinutes;
                            FinishTimes[j][i] = 0;
                            
                        }
                        
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    if (Working[i] > 0)
                    {
                        break;
                    }
                    else if(i==5)
                    {
                        finished = true;
                    }
                }
                TimeInMinutes += 0.5;
              




            }

            
        }

        private static List<string[]> GetScheduleData(int pieces,string Identifier)
        {
            int[][] ScheduleStart = new int[6][];
            int[][] ScheduleEnd = new int[6][];
            for (int i = 0; i < StartSchedule.Length; i++)
            {
                ScheduleStart[i] = new int[StartSchedule[i].Length];
                ScheduleEnd[i] = new int[EndSchedule[i].Length];
                for (int j = 0; j < ScheduleStart[i].Length; j++)
                {
                    ScheduleStart[i][j] = (int)Math.Ceiling(StartSchedule[i][j]);
                    ScheduleEnd[i][j] = (int)Math.Ceiling(EndSchedule[i][j]);
                }
            }
            List<string[]> output = new List<string[]>();
            int daysElapsed = (ScheduleEnd[0][0] - ScheduleStart[0][0]) / 960;
            if (TimeInMinutes % 960 + (ScheduleEnd[0][0] - ScheduleStart[0][0]) % 960 >= 960)
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

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }

                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Vágó-2" && j == 0)
                        {
                            output.Add(new string[5]);
                            
                            
                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][1]);
                            output.Last()[1] = WorkStations[i][counter];
                            output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                            output.Last()[3] = ConvertToDate(ScheduleEnd[i][1]).Split(' ')[1];
                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Hajlító-1")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }

                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Hajlító-2")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][1] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                if (pieces % 2 == 0)
                                {
                                    output.Last()[3] = ConvertToDate(ScheduleEnd[i][1]).Split(' ')[1];
                                    output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                                }
                                else
                                {
                                    output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                                    output.Last()[3] = ConvertToDate((ScheduleEnd[i][1])).Split(' ')[1];
                                }
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                if (pieces % 2 == 0)
                                {
                                    output.Last()[3] = ConvertToDate(ScheduleEnd[i][1]).Split(' ')[1];
                                    output.Last()[2] = "6:00";
                                }
                                else
                                {
                                    output.Last()[2] = "6:00";
                                    output.Last()[3] = ConvertToDate((ScheduleEnd[i][1])).Split(' ')[1];
                                }
                            }

                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Hegesztő-1")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }

                            output.Last()[4] = Identifier;
                            
                            

                        }
                        else if (WorkStations[i][counter] == "Hegesztő-2")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                if (pieces % 2 == 0)
                                {
                                    output.Last()[3] = ConvertToDate(ScheduleEnd[i][1]).Split(' ')[1];
                                    output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                                }
                                else
                                {
                                    output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                                    output.Last()[3] = ConvertToDate((ScheduleEnd[i][1])).Split(' ')[1];
                                }
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][1]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                if (pieces % 2 == 0)
                                {
                                    output.Last()[3] = ConvertToDate(ScheduleEnd[i][1]).Split(' ')[1];
                                    output.Last()[2] = "6:00";
                                }
                                else
                                {
                                    output.Last()[2] = "6:00";
                                    output.Last()[3] = ConvertToDate((ScheduleEnd[i][1])).Split(' ')[1];
                                }
                            }

                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Tesztelő")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Festő-1")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Festő-2")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][1] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Festő-3")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][2] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Csomagoló-1")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][0] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }
                        else if (WorkStations[i][counter] == "Csomagoló-2")
                        {
                            output.Add(new string[5]);

                            output.Last()[0] = ConvertToDateWithYear(ScheduleStart[i][1] + j * 960);

                            output.Last()[1] = WorkStations[i][counter];
                            if (j != 0 && j != daysElapsed)
                            {
                                output.Last()[2] = "6:00";
                                output.Last()[3] = "22:00";
                            }
                            else if (j == 0 && j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == 0)
                            {
                                output.Last()[3] = "22:00";
                                output.Last()[2] = ConvertToDate(ScheduleStart[i][0]).Split(' ')[1];
                            }
                            else if (j == daysElapsed)
                            {
                                output.Last()[3] = ConvertToDate(ScheduleEnd[i][0]).Split(' ')[1];
                                output.Last()[2] = "6:00";
                            }


                            output.Last()[4] = Identifier;
                        }

                    }


                    counter++;

                }
            }

            return output;
        }

        public static string ConvertToDate(int mins)
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
            return new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortDateString().Substring(6).RemoveWhitespace() + " " + new DateTime(dateTime[4], dateTime[0], dateTime[1], dateTime[2], dateTime[3], 0).ToShortTimeString();
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
    }
}
