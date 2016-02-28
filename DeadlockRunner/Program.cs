using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadlockRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfIterations = 10;
            for (int i = 1; i <= numberOfIterations; i++)
            {
                int numberOfPhilo = 5;
                int thinkingTime = 10;
                int eatingTime = 10;
                int deadlockSafe = 0;

                var proc = new Process();
                proc.StartInfo.FileName = "ue1.exe";
                proc.StartInfo.Arguments = String.Format("{0} {1} {2} {3} {4}", numberOfPhilo, thinkingTime, eatingTime, deadlockSafe, i);
                proc.Start();
                //proc.WaitForExit();
                //var exitCode = proc.ExitCode;
                //proc.Close();
            }
        }
    }
}
