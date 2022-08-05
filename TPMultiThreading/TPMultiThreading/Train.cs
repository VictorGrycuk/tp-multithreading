using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TPMultiThreading
{
    public class Train
    {
        public int Number { get; set; }
        public int CurrentCapacity { get; set; }
        public int Speed { get; set; }
        public int BaseWaitingTime { get; set; }
        public string Status { get; set; }
        public bool ForwardDirection = true;
        public bool Waiting;
        public Station CurrentStation { get; set; }

        public Label TrainName;
        public Label LabelNumber;
        public Label LabelCurrentStation;
        public Label LabelDestination;
        public Label LabelStatus;
        public Label LabelCurrentCapacity;

        private static readonly object Locker = new object();
        private static Random _rnd;
        
        public Train()
        {
            _rnd = new Random();
            CurrentCapacity = 0;
        }

        public void Start()
        {
            Central.UpdateMap(this);
            Process();
        }

        public void Process()
        {
            while (true)
            {
                UnloadPeople();
                LoadPeople();
                GoNextStation();
            }
        }

        private void UnloadPeople()
        {
            var peopleLeaving = _rnd.Next(0, CurrentCapacity);
            Report($"Unloading { peopleLeaving } people");
            CurrentCapacity -= peopleLeaving;
            Thread.Sleep((peopleLeaving + 1) * Speed);
        }

        private void LoadPeople()
        {
            var peopleEntering = _rnd.Next(0, CurrentStation.WaitingPeople);
            Report($"Loading { peopleEntering } people");
            CurrentCapacity += peopleEntering;
            Thread.Sleep((peopleEntering + 1) * Speed);
        }

        public void Report(string status)
        {
            Status = status;
            Central.UpdateStatus(this);
        }

        private void GoNextStation()
        {
            ChangeDirection();

            var nextStation = ForwardDirection ? CurrentStation.NextStation : CurrentStation.PreviousStation;

            lock (Locker)
            {
                Report("Checking availability for " + nextStation.Name);
                while (ForwardDirection ? nextStation.ForwardTrack != null : nextStation.BackwardTrack != null)
                {
                    Waiting = true;
                    LabelStatus.ForeColor = Color.Red;
                    Report(nextStation.Name + " is busy. Waiting for it to be available");
                    Monitor.Wait(Locker, GetWaitingTime());
                    LabelStatus.ForeColor = Color.DarkGreen;
                }

                Report("Greenlighted for " + nextStation.Name);
                LabelStatus.ForeColor = Color.Black;
                Waiting = false;

                // Notifying central that the train in the previous station is ok to proceed,
                // if there is one waiting
                if (CheckWaitingTrain())
                {
                    Report("Greenlighting waiting train");
                    Monitor.PulseAll(Locker);
                }

                Report("Leaving station " + CurrentStation.Name);
                SwitchStation(nextStation);

                Report("Arriving at " + CurrentStation.Name);
                CurrentStation = nextStation;
                Central.UpdateMap(this);
            }
        }

        private void ChangeDirection()
        {
            if (ForwardDirection && CurrentStation.NextStation == null)
            {
                Report("Changing direction at" + CurrentStation.Name);
                ForwardDirection = false;
            }
            else if (!ForwardDirection && CurrentStation.PreviousStation == null)
            {
                Report("Changing direction at " + CurrentStation.Name);
                ForwardDirection = true;
            }
        }

        private void SwitchStation(Station nextStation)
        {
            var isTerminal = CurrentStation.NextStation == null || CurrentStation.PreviousStation == null;

            if (ForwardDirection && !isTerminal)
            {
                CurrentStation.ForwardTrack = null;
                nextStation.ForwardTrack = this;
            }
            else if (ForwardDirection && isTerminal)
            {
                // Edge case for when starting station is a terminal
                if (CurrentStation.ForwardTrack != null && CurrentStation.ForwardTrack.Number == Number)
                {
                    CurrentStation.ForwardTrack = null;
                    nextStation.ForwardTrack = this;
                    return;
                }

                CurrentStation.BackwardTrack = null;
                nextStation.ForwardTrack = this;
            }
            else if (!ForwardDirection && !isTerminal)
            {
                CurrentStation.BackwardTrack = null;
                nextStation.BackwardTrack = this;
            }
            else if (!ForwardDirection && isTerminal)
            {
                CurrentStation.ForwardTrack = null;
                nextStation.BackwardTrack = this;
            }
        }

        private bool CheckWaitingTrain()
        {
            var isTerminal = CurrentStation.NextStation == null || CurrentStation.PreviousStation == null;

            if (ForwardDirection && !isTerminal)
            {
                return CurrentStation.PreviousStation.ForwardTrack != null &&
                        CurrentStation.PreviousStation.ForwardTrack.Waiting;
            }

            if (ForwardDirection && isTerminal)
            {
                return CurrentStation.NextStation.BackwardTrack != null &&
                        CurrentStation.NextStation.BackwardTrack.Waiting;
            }

            if (!ForwardDirection && !isTerminal)
            {
                return CurrentStation.NextStation.BackwardTrack != null &&
                        CurrentStation.NextStation.BackwardTrack.Waiting;
            }

            if (!ForwardDirection && isTerminal)
            {
                return CurrentStation.PreviousStation.ForwardTrack != null &&
                        CurrentStation.PreviousStation.ForwardTrack.Waiting;
            }

            return false;
        }

        private int GetWaitingTime()
        {
            var normalizedTime = (decimal)CurrentCapacity / 100;
            return (int)normalizedTime * BaseWaitingTime;
        }
    }
}
