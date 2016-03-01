using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets;

namespace ue1
{
    /// <summary>
    /// Help Class to provide a function to get overall runtime of Application
    /// </summary>
    public static class Help
    {
        static Stopwatch sw = new Stopwatch();

        public static void Init()
        {
            sw.Start();
        }

        public static string GetRuntime()
        {
            return sw.ElapsedMilliseconds.ToString();
        }

        public static long GetRuntimeLong()
        {
            return sw.ElapsedMilliseconds;
        }
    }

    /// <summary>
    /// The forkstore holds the array of forks/mutexes and is missused to calc the sum of eatingTime
    /// </summary>
    public static class ForkStore
    {
        public static Mutex[] forks;
        public static int[] eatingTimeSum;
        public static int[] eatingCount;

        public static void Init(int numberOfPhilos)
        {
            forks = new Mutex[numberOfPhilos];
            eatingTimeSum = new int[numberOfPhilos];
            eatingCount = new int[numberOfPhilos];

            for (int i = 0; i < forks.Length; i++)
            {
                forks[i] = new Mutex();
                eatingTimeSum[i] = 0;
                eatingCount[i] = 0;
            }
        }

        public static void AddEatingTime(int index,int eatingTime)
        {
            eatingTimeSum[index] = eatingTimeSum[index] + eatingTime;
            eatingCount[index]++;
        }

        public static string EatingSumOfAllPhilo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("EatingSum: ");
            for (int i = 0; i < eatingTimeSum.Length; i++)
            {
                sb.AppendLine(String.Format("[P: {0}, Sum: {1}, Count: {2}]; ", i, eatingTimeSum[i], eatingCount[i]));
            }
            return sb.ToString();
        }
    }

    public class Philosopher
    {
        private int _maxThinkingTime;
        private int _maxEatingTime;
        private int _index;
        private Random rand;
        public bool run = true;
        public Func<int> firstFork;
        public Func<int> secondFork;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public Philosopher(int index, int maxThinkingTime, int maxEatingTime)
        {
            _index = index;
            rand = new Random();
            _maxThinkingTime = maxThinkingTime;
            _maxEatingTime = maxEatingTime;
            firstFork = () => { return _index; };
            secondFork = () => { return (_index+1)%ForkStore.forks.Length; };
        }

        public void Work()
        {
            while (run)
            {
                var thinkingTime = rand.Next(0, _maxThinkingTime);
                _logger.Info("{0}: Phil_{1} is thinking for {2}ms...", Help.GetRuntime(), _index, thinkingTime);
                Thread.Sleep(thinkingTime);
                _logger.Info("{0}: Phil_{1} wants to eat now", Help.GetRuntime(), _index);

                var indexFirstFork = firstFork();
                ForkStore.forks[indexFirstFork].WaitOne();
                _logger.Info("{0}: Phil_{1} took fork {2}", Help.GetRuntime(), _index, indexFirstFork);

                var indexSecondFork = secondFork();

                ForkStore.forks[indexSecondFork].WaitOne();
            
                _logger.Info("{0}: Phil_{1} took fork {2}", Help.GetRuntime(), _index, indexSecondFork);

                var eating_time = rand.Next(0, _maxEatingTime);
                Thread.Sleep(eating_time);
                ForkStore.AddEatingTime(_index, eating_time);
                _logger.Info("{0}: Phil_{1} is done eating. Puts back Fork {2} and {3}. Eatingtime {4}ms", Help.GetRuntime(), _index, indexFirstFork, indexSecondFork, eating_time);
                _logger.Info(ForkStore.EatingSumOfAllPhilo());

                ForkStore.forks[indexFirstFork].ReleaseMutex();
                ForkStore.forks[indexSecondFork].ReleaseMutex();
            }
            _logger.Info("{0}: Phil_{1} stopped", Help.GetRuntime(), _index); 
        }
    }

    public class Program
    {
        static List<Philosopher> philosophers_list = new List<Philosopher>();
        static List<Thread> threads_philosophers_list = new List<Thread>();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static int _runtime = 480000; // 8 min in ms
        //private static int _runtime = 10000; // 10 sec in ms

        public static void Main(string[] args)
        {
            int numberOfPhilos = 0;
            int thinkingtime = 0;
            int eatingtime = 0;
            int deadlockSafe = 0;
            int numberOfIteration = 1;

            //if args is set then the program is started via the DeadlockRunner - Project
            if (args.Length == 5)
            {
                numberOfPhilos = int.Parse(args[0]);
                thinkingtime = int.Parse(args[1]);
                eatingtime = int.Parse(args[2]);
                deadlockSafe = int.Parse(args[3]);
                numberOfIteration = int.Parse(args[4]);
            }
            else
            {
                Console.WriteLine("Number of Philosopher: ");
                ReadInt("Number of Philosopher", out numberOfPhilos);
                Console.WriteLine("Thinkingtime (Milliseconds): ");
                ReadInt("Thinkingtime", out thinkingtime);
                Console.WriteLine("Eatingtime (Milliseconds): ");
                ReadInt("Eatingtime", out eatingtime);
                // Deadlock safe
                Console.WriteLine("Deadlock safe? 0 = false, 1 = true: ");
                ReadInt("DeadlockSafe", out deadlockSafe);   
            }

            //Configure Log File Name
            var target = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
            target.FileName = String.Format("logs/{0}-{1}-{2}-{3}/Phil-{0}-Think-{1}-Eat-{2}-DLSafe-{3}-iter-{4}_{5}.log", numberOfPhilos, thinkingtime, eatingtime, deadlockSafe, numberOfIteration, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            LogManager.ReconfigExistingLoggers();

            _logger.Info("Number of Philos: {0}, ThinkingTimeMax: {1}, EatingTimeMax: {2},DeadlockSafe: {3}", numberOfPhilos, thinkingtime, eatingtime, deadlockSafe);

            Console.WriteLine("Philosophers are started now - To stop them just press ENTER");

            ForkStore.Init(numberOfPhilos);
            Help.Init();

            ConcurrentBag<Philosopher> philosophers_bag = new ConcurrentBag<Philosopher>();
            ConcurrentBag<Thread> threads_philosophers_bag = new ConcurrentBag<Thread>();

            Parallel.For(0,numberOfPhilos, i =>
            {
                Philosopher phil = new Philosopher(i, thinkingtime, eatingtime);

                // Anti Deadlock -> righthanded Philo
                if (i == 0 && deadlockSafe == 1)
                {
                    phil.firstFork = () => { return (i + 1)%ForkStore.forks.Length; };
                    phil.secondFork = () => { return i; };
                }

                Thread tphil = new Thread(new ThreadStart(phil.Work));
                tphil.Start();

                philosophers_bag.Add(phil);
                threads_philosophers_bag.Add(tphil);
            });

            philosophers_list = philosophers_bag.ToList();

            //Stopp the main Thread for the _runtime Value
            while (Help.GetRuntimeLong() < _runtime)
            {
                Thread.Sleep(30000); // 30 sec
            }

            //Stopp the loops of the philosophers
            Parallel.ForEach(philosophers_list, phil => 
                {
                    phil.run = false;
                });


            //Join the Philosopher Threads
            foreach (var tphil in threads_philosophers_list)
            {
                tphil.Join();
            }

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine("Stopping all philosopher threads");
            //Console.WriteLine("Press ENTER to exit programm...");
           // Console.Beep();

            _logger.Info("-----------------------------");
            _logger.Info("Stopping all philosopher threads");
            _logger.Info("Press ENTER to exit programm...");
            Environment.Exit(1);
        }

        static void ReadInt(string errorText,out int target)
        {
            if(!Int32.TryParse(Console.ReadLine(),out target))
            {
                Console.WriteLine("Failed to parse input for " + errorText);
                Console.WriteLine("***Stopped execution of programm***");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        public string ProjectPath()
        {
            return Environment.CurrentDirectory;
        }
    }
}
