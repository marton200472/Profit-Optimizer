using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;

namespace ProfitOptimizer
{
    public static class CsvWriter
    {
        public static void WriteOrdersToCsv(string[][] input)
        {
            string[][] AllData = new string[input.Length+1][];
            AllData[0] = new string[] {"Megrendelésszám","Profit összesen","Levont kötbér","Munka megkezdése","Készre jelentés ideje","Megrendelés eredeti határideje" };
            for (int i = 0; i < input.Length; i++)
            {
                AllData[i + 1] = input[i];
            }
            for (int i = 1; i < AllData.Length-1; i++)
            {
                for (int j = i+1; j < AllData.Length; j++)
                {
                    if (AllData[i][0].CompareTo(AllData[j][0])>0)
                    {
                        var tmp = AllData[i];
                        AllData[i] = AllData[j];
                        AllData[j] = tmp;
                    }
                }
            }
            string[] DataToBeWritten = new string[AllData.Length];
            for (int i = 0; i < DataToBeWritten.Length; i++)
            {
                DataToBeWritten[i] = "";
                for (int j = 0; j < 6; j++)
                {
                    DataToBeWritten[i] += AllData[i][j] + ";";
                }
                DataToBeWritten[i] = DataToBeWritten[i].Substring(0, DataToBeWritten[i].Length-1);
            }
            
            SaveFileDialog saveFileDialog = new SaveFileDialog() {AddExtension=true, DefaultExt=".csv",Filter= "csv files(*.csv)|*.csv",Title="Rendelésadatok mentése"};
            Console.WriteLine("Optimalizálás sikeres. A rendelésadatok mentéséhez nyomd meg bármely gombot.");
            Console.ReadKey();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllLines(saveFileDialog.FileName, DataToBeWritten, Encoding.UTF8);
                    Console.WriteLine("Rendelésadatok mentve.");
                }
                catch (Exception)
                {

                    Logger.LogEntry("A mentés sikertelen. Valószínűleg egy másik folyamat használja a fájlt.");
                    Console.WriteLine("Rendelésadatok mentése sikertelen.");
                }
            }
            else
            {
                Console.WriteLine("A rendelésadatok mentését a felhasználó megszakította.");
            }
            
            
            
        }
        public static void WriteWorkToCsv(List<string[]> input)
        {
            string[][] AllData = new string[input.Count()][];
            
            string[] Header = new string[] { "Dátum;Gép;Kezdő időpont;Záró időpont;Megrendelésszám" };
            for (int i = 0; i < input.Count(); i++)
            {
                AllData[i] = input[i].ToArray();
            }
            for (int i = 1; i < AllData.Length - 1; i++)
            {
                for (int j = i + 1; j < AllData.Length; j++)
                {
                    if (AllData[i][0].CompareTo(AllData[j][0])>0)
                    {
                        var tmp = AllData[i];
                        AllData[i] = AllData[j];
                        AllData[j] = tmp;
                    }
                }
            }
            int[] workstationindexes = new int[input.Count()];
            for (int i = 0; i < workstationindexes.Length; i++)
            {
                workstationindexes[i] = GetWorkstationIndex(AllData[i]);
            }
            for (int i = 1; i < AllData.Length - 1; i++)
            {
                for (int j = i + 1; j < AllData.Length; j++)
                {
                    
                    if (AllData[i][0]==AllData[j][0]&&workstationindexes[i]>workstationindexes[j])
                    {
                        var tmp = AllData[i];
                        AllData[i] = AllData[j];
                        AllData[j] = tmp;

                        int temp = workstationindexes[i];
                        workstationindexes[i] = workstationindexes[j];
                        workstationindexes[j] = temp;
                    }
                }
            }
            string[] DataToBeWritten = new string[AllData.Length];
            
            for (int i = 0; i < DataToBeWritten.Length; i++)
            {
                DataToBeWritten[i] = "";
                for (int j = 0; j < 5; j++)
                {
                    DataToBeWritten[i] += AllData[i][j] + ";";
                }
                DataToBeWritten[i] = DataToBeWritten[i].Substring(0, DataToBeWritten[i].Length - 1);
            }
            
            
            SaveFileDialog saveFileDialog = new SaveFileDialog() { AddExtension = true, DefaultExt = ".csv", Filter = "csv files(*.csv)|*.csv",Title="Munkarend mentése" }; 
            Console.WriteLine("A munkarend mentéséhez nyomd meg bármely gombot.");
            Console.ReadKey();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllLines(saveFileDialog.FileName, Header, Encoding.UTF8);
                    File.AppendAllLines(saveFileDialog.FileName, DataToBeWritten, Encoding.UTF8);
                    Console.WriteLine("Munkarend mentve.");
                }
                catch (Exception)
                {
                    Logger.LogEntry("A mentés sikertelen. Valószínűleg egy másik folyamat használja a fájlt.");
                    Console.WriteLine("Munkarend mentése sikertelen.");

                }
            }
            else
            {
                Console.WriteLine("A munkarend mentését a felhasználó megszakította.");
            }
            

        }

        private static int GetWorkstationIndex(string[] row)
        {
            int workstationindex = 0;
            for (int k = 0; k < Processor.WorkStations.Length; k++)
            {
                for (int l = 0; l < Processor.WorkStations[k].Length; l++)
                {
                    if (row[1] == Processor.WorkStations[k][l])
                    {

                        return workstationindex;
                    }
                    else
                    {
                        workstationindex++;
                    }
                }
            }
            return 0;
        }

    }
}
