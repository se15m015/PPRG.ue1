using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ue1
{

    public class Philosopher
    {
        private int _thinkingTime;
        private int _eatingTime;

        public Philosopher(int t1, int t2)
        {
            Random rnd = new Random();
            _thinkingTime = rnd.Next(0, t1);
            _eatingTime = rnd.Next(0, t2);

            //_thinkingTime = t1;
            //_eatingTime = t2;

        }

        public void Work()
        {
            while (true)
            { 
                Console.Out.WriteLine("t1 {0}", _thinkingTime);
                Console.Out.WriteLine("t2 {0}", _eatingTime);
                Console.Out.WriteLine("Phil1 thinking...");
                Thread.Sleep(_thinkingTime);
                Console.Out.WriteLine("Phil1 wants to eat now");
                
                Console.Out.WriteLine("Phil1 wants to take fork 0");
                //take fork 
                Console.Out.WriteLine("Phil1 wants to take fork 1");
                //take fork
                Thread.Sleep(_eatingTime);
                Console.Out.WriteLine("Phil1 is done with eating");
                //put forks back
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine("Number of Philosoper: ");
            int n = Int32.Parse(Console.ReadLine());
            Console.Out.WriteLine("Thinkingtime (Milliseconds): ");
            int t1 = Int32.Parse(Console.ReadLine());
            Console.Out.WriteLine("Eatingtime (Milliseconds): ");
            int t2 = Int32.Parse(Console.ReadLine());

            Console.Out.WriteLine("{0}, {1}, {2}", n, t1, t2);
            Philosopher phil1 = new Philosopher(t1, t2);
            //Thread tphil1 = new Thread(new ThreadStart(phil1.Work));
            Thread tphil1 = new Thread(new ThreadStart(Abc));

            tphil1.Start();
        }

        public static void Abc()
        {
        }

    }
}
