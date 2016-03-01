using System;
using System.Collections;
using System.Collections.Concurrent;
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
            var philoValues = new int[] {8};
            var thinkingValues = new int[] {50};
            var eatingValues = new int[] {50};
            int deadlockSafe = 0;
            int maxConcurrentProc = 20;
            int numberOfIterations = 10;

            var procBag = new ConcurrentBag<Process>();

            for (int p = 0; p < philoValues.Length; p++)
            {
                for (int t = 0; t < thinkingValues.Length; t++)
                {
                    for (int e = 0; e < eatingValues.Length; e++)
                    {
                        for (int i = 1; i <= numberOfIterations; i++)
                        {
                            var proc = new Process();
                            proc.StartInfo.FileName = "ue1.exe";
                            proc.StartInfo.Arguments = String.Format("{0} {1} {2} {3} {4}", philoValues[p], thinkingValues[t], eatingValues[e], deadlockSafe, i);
                            proc.Start();

                            procBag.Add(proc);

                            if (maxConcurrentProc !=-1 && procBag.Count >= maxConcurrentProc)
                            {
                                foreach (var process in procBag)
                                {
                                    process.WaitForExit();
                                    //var exitCode = proc.ExitCode;
                                    process.Close();
                                }
                                procBag = new ConcurrentBag<Process>();
                            }
                            //Console.WriteLine("{0} {1} {2} {3}", philoValues[p], thinkingValues[t], eatingValues[e], i);
                        }
                    }
                }
            }

            Console.WriteLine("Press any Key to Exit...");
            Console.ReadKey();



            //for (int i = 1; i <= numberOfIterations; i++)
            //{
            //    int numberOfPhilo = 5;
            //    int thinkingTime = 10;
            //    int eatingTime = 10;
            //    int deadlockSafe = 0;

            //    var proc = new Process();
            //    proc.StartInfo.FileName = "ue1.exe";
            //    proc.StartInfo.Arguments = String.Format("{0} {1} {2} {3} {4}", numberOfPhilo, thinkingTime, eatingTime, deadlockSafe, i);
            //    proc.Start();
            //    //proc.WaitForExit();
            //    //var exitCode = proc.ExitCode;
            //    //proc.Close();
            //}

        }
    }
}
