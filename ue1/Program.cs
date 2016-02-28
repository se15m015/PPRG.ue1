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

namespace ue1
{
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
    }

    public static class ForkStore
    {
        public static Mutex[] forks;

        public static void Init(int numberOfPhilos)
        {
            forks = new Mutex[numberOfPhilos];
            for (int i = 0; i < forks.Length; i++)
            {
                forks[i] = new Mutex();
            }
        }
    }

    public class Philosopher
    {
        private int _maxThinkingTime;
        private int _maxEatingTime;
        private int _index;
        private Random rand;
        public bool run = true;

        public Philosopher(int index, int maxThinkingTime, int maxEatingTime)
        {
            _index = index;
            rand = new Random();
            _maxThinkingTime = maxThinkingTime;
            _maxEatingTime = maxEatingTime;
        }

        public void Work()
        {
            while (run)
            {
                var thinkingTime = rand.Next(0, _maxThinkingTime);
                Console.WriteLine("{0}: Phil{1} is thinking for {2}ms...", Help.GetRuntime(), _index, thinkingTime);
                Thread.Sleep(thinkingTime);
                Console.WriteLine("{0}: Phil{1} wants to eat now", Help.GetRuntime(), _index);

                ForkStore.forks[_index].WaitOne();
                Console.WriteLine("{0}: Phil{1} took fork {2}", Help.GetRuntime(), _index, _index);


                var indexSecondFork = (_index-1)%ForkStore.forks.Length;
                if(indexSecondFork < 0)
                {
                    indexSecondFork = indexSecondFork + ForkStore.forks.Length;
                }

                ForkStore.forks[indexSecondFork].WaitOne();
                Console.WriteLine("{0}: Phil{1} took fork {2}", Help.GetRuntime(), _index, indexSecondFork);

                var eating_time = rand.Next(0, _maxEatingTime);
                Thread.Sleep(eating_time);
                Console.WriteLine("{0}: Phil{1} is done eating. Took {2}ms", Help.GetRuntime(), _index, eating_time);

                ForkStore.forks[_index].ReleaseMutex();
                ForkStore.forks[indexSecondFork].ReleaseMutex();

            }
            Console.WriteLine("{0}: Phil{1} stopped", Help.GetRuntime(), _index);
            
        }
    }

    public class Program
    {
        static List<Philosopher> philosophers_list = new List<Philosopher>();
        static List<Thread> threads_philosophers_list = new List<Thread>();

        public static void Main(string[] args)
        {
            
            Console.WriteLine("Number of Philosopher: ");
            int numberOfPhilos = 0;
            ReadInt("Number of Philosopher", out numberOfPhilos);
            Console.WriteLine("Thinkingtime (Milliseconds): ");
            int thinkingtime = 0;
            ReadInt("Thinkingtime", out thinkingtime);
            Console.WriteLine("Eatingtime (Milliseconds): ");
            int eatingtime = 0;
            ReadInt("Eatingtime", out eatingtime);

            Console.WriteLine("{0}, {1}, {2}", numberOfPhilos, thinkingtime, eatingtime);

            Console.WriteLine("Philosophers are started now - To stop them just press ENTER");

            ForkStore.Init(numberOfPhilos);
            Help.Init();

            ConcurrentBag<Philosopher> philosophers_bag = new ConcurrentBag<Philosopher>();
            ConcurrentBag<Thread> threads_philosophers_bag = new ConcurrentBag<Thread>();

            Parallel.For(0,numberOfPhilos, i =>
            {
                Philosopher phil = new Philosopher(i, thinkingtime, eatingtime);
                Thread tphil = new Thread(new ThreadStart(phil.Work));
                tphil.Start();

                philosophers_bag.Add(phil);
                threads_philosophers_bag.Add(tphil);
            });

            philosophers_list = philosophers_bag.ToList();

            Console.ReadLine();

            Parallel.ForEach(philosophers_list, phil => 
                {
                    phil.run = false;
                });


            foreach (var tphil in threads_philosophers_list)
            {
                tphil.Join();
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Stopping all philosopher threads");
            Console.WriteLine("Press ENTER to exit programm...");
            Console.ReadLine();
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
    }
}
