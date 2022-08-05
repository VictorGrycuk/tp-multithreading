using System;
using System.Drawing;

namespace TPMultiThreading
{
    public class Station
    {
        public string Name { get; set; }
        public int WaitingPeople => GetWaitingPeople();
        public Train ForwardTrack { get; set; }
        public Train BackwardTrack { get; set; }
        public Station PreviousStation { get; set; }
        public Station NextStation { get; set; }
        public Point MapLocation { get; set; }

        private static Random rnd;

        public Station()
        {
            rnd = new Random();
        }

        private static int GetWaitingPeople()
        {
            return rnd.Next(0, 50); 
        }
    }
}
