using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ProfitOptimizer
{
    public static class Logger
    {
        private static string _fileName;
        public static string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName =  "Log/Optimizer" + value.Replace('.','_') + ".log";
            }
        }

        public static void LogEntry(string contents) {
            string[] logentry = new string[] { DateTime.Now.ToShortTimeString()+":"+DateTime.Now.Second+" >\t"+contents };
            for (; ; )
            {
                try
                {
                    File.AppendAllLines(FileName, logentry, Encoding.UTF8);
                    break;
                }
                catch (Exception)
                {

                    
                }
            }
            
        }

        public static void Initialize()
        {
            FileName = DateTime.Today.ToShortDateString().RemoveWhitespace() + DateTime.Now.ToShortTimeString().Replace(':','-');
            if (!Directory.Exists(Directory.GetCurrentDirectory()+@"\Log"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Log");
            }
            int i = 0;
            string name = FileName.Split('.')[0];
            while (File.Exists(FileName))
            {
                i++;
                _fileName =  name+ "("+i+")" + ".log";
            }
        }

        
        
    }
}
