using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace ProfitOptimizer
{
    public class Order
    {
        public string Identifier;

        public ProductTypes ProductType;

        public int Pieces;

        public int TimeLeft;

        public int IncomePerPiece;

        public int DelayPenalty;

        public double Priority;

        public int TimeToComplete;

        public int TimeToFinishCutting;
    }

    public enum ProductTypes
    {
        GYB,
        SB,
        FB,
        Error
    }

    public static class CsvLoader
    {
        public static Order[] GetInputFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "csv files(*.csv)|*.csv|txt files(*.txt)|*.txt", FilterIndex = 0,Multiselect=false,Title="Forrásfájl kiválasztása" };

            openFileDialog.ShowDialog();
            
            if (File.Exists(openFileDialog.FileName))
            {
                Logger.LogEntry("Forrásfájl kiválasztva.");
                return GetSplitter(openFileDialog.FileName);
            }
            else
            {
                Console.WriteLine("A forrásfájl nem létezik, vagy helytelenül adtad meg! Próbáld újra!");
                return GetInputFile();
            }
            
        }

        public static Order[] GetSplitter(string path)
        {
            Console.WriteLine("Add meg az elválasztó karaktert!");
            var splitter = Console.ReadLine();
            if (splitter.Length>0&&splitter.Length<2)
            {
                Logger.LogEntry("Elválasztó karakter kiválasztva.");
                return GetData(path, splitter[0]);
            }
            else
            {
                if (splitter=="\t")
                {
                    Logger.LogEntry("Elválasztó karakter kiválasztva.");
                    return GetData(path,'\t');
                }
                else
                {
                    Console.WriteLine("Az elválasztó karaktert hibásan adtad meg!");
                    return GetSplitter(path);
                }
                
            }
        }
        //feltételezzük, hogy az adatforrás fejlécet tartalmaz, és utf-8 kódolású
        private static Order[] GetData(string path,char splitter)
        {
            
            string[] AllData = File.ReadAllLines(path,Encoding.UTF8);
            string[][] SplittedData = new string[AllData.Length-1][];
            for (int i = 0; i < AllData.Length-1; i++)
            {
                var splitted = AllData[i+1].Split(splitter);
                SplittedData[i] = new string[6];
                for (int j = 0; j < 6; j++)
                {
                    SplittedData[i][j] = splitted[j];
                }
                if (splitted.Length!=6)
                {
                    Logger.LogEntry("Hibás forrásfájl vagy elválasztó karakter!");
                    Program.Error();
                }
            }
            Logger.LogEntry("Adatok beolvasása a forrásfájlból sikeres.");
            return ConvertToOrder(SplittedData);
        }

        private static Order[] ConvertToOrder(string[][] splitted)
        {
            int[] TimeNow = new int[] { 7, 20, 6, 0 };
            Order[] orders = new Order[splitted.Length];
            for (int i = 0; i < splitted.Length; i++)
            {
                var type = splitted[i][1] == "SB" ? ProductTypes.SB : (splitted[i][1] == "FB" ? ProductTypes.FB : (splitted[i][1] == "GYB" ? ProductTypes.GYB : ProductTypes.Error));
                if (type == ProductTypes.Error) { Logger.LogEntry("Probléma adódott egy rendelés típusával kapcsolatban."); throw new NotImplementedException(); }

                splitted[i][2] = splitted[i][2].RemoveWhitespace();
                int pieces;
                if (!int.TryParse(splitted[i][2], out pieces))
                {
                    Logger.LogEntry("Hiba adódott egy rendelés darabszámával kapcsolatban.");
                    throw new Exception();
                }

                var tmp = splitted[i][3].Split(' ');
                if (tmp.Length != 2)
                {
                    Logger.LogEntry("Nem megfelelő a határidő dátum-idő formátuma. A helyes formátum: HH.NN. ÓÓ:PP");
                    throw new Exception();
                }
                var month = tmp[0].Split('.')[0];
                var day = tmp[0].Split('.')[1];
                var hour = tmp[1].Split(':')[0];
                var minute = tmp[1].Split(':')[1];
                minute = minute.Substring(0,2);
                int monthInt, dayInt, hourInt, minuteInt;
                if (month.Length>2)
                {
                    throw new Exception();
                }
                try
                {
                    monthInt = int.Parse(month);
                    dayInt = int.Parse(day);
                    hourInt = int.Parse(hour);
                    minuteInt = int.Parse(minute);
                }
                catch (Exception)
                {
                    Logger.LogEntry("Nem megfelelő a határidő dátum-idő formátuma. A helyes formátum: HH.NN. ÓÓ:PP");
                    throw;
                }
                int[] timeleft = new int[] { monthInt - TimeNow[0], dayInt - TimeNow[1], hourInt - TimeNow[2], minuteInt - TimeNow[3] };
                int daysInMonths = 0;
                for (int j = TimeNow[0] + 1; j <= monthInt; j++)
                {
                    
                    
                    daysInMonths += DateTime.DaysInMonth(2020,j);

                }
                
                int minutesleft = (daysInMonths + timeleft[1]) * 16 * 60;
                minutesleft += timeleft[2] * 60;
                minutesleft += timeleft[3];

                splitted[i][4]=splitted[i][4].RemoveWhitespace();
                
                int income = int.Parse(splitted[i][4]);

                
                splitted[i][5]=splitted[i][5].RemoveWhitespace();
                int penalty = int.Parse(splitted[i][5]);
                
                orders[i] = new Order() { Identifier = splitted[i][0], ProductType = type, Pieces = pieces, TimeLeft = minutesleft,IncomePerPiece=income,DelayPenalty=penalty};
            }
            Logger.LogEntry("Rendelések létrehozása sikeres.");
            return orders;
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }

    
}
