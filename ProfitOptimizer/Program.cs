using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ProfitOptimizer
{
    
    class Program
    {
        
        public static string[][] FirstOutput;
        [STAThread]
        static void Main(string[] args)
        {
            

            try
            {
                Logger.Initialize();
                Logger.LogEntry("Logger sikeresen elindítva.");
            }
            catch (Exception e)
            {

                MessageBox.Show("A logger indítása sikertelen volt. "+e.Message);
                Environment.Exit(1);
            }


            if (args.Length>0)
            {
                SequenceFinder.Optimize(args[0]);
            }
            else
            {
                SequenceFinder.Optimize();
            }
            

            

            Console.ReadKey();
        }

        public static void Error()
        {
            if (MessageBox.Show("Megnyitod a log-ot?", "Hiba", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Process.Start(Directory.GetCurrentDirectory() + "/" + Logger.FileName);
            }
            Environment.Exit(1);
        }

        
    }
}
