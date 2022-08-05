using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace TPMultiThreading
{
    public class Central
    {
        public List<Station> Stations = new List<Station>();
        private static readonly Queue<Train> _trainDeposit = new Queue<Train>();
        private static readonly List<Train> _activeTrains = new List<Train>();
        public static readonly object Locker = new object();

        public Central()
        {
            var stationList = new List<string>
            {
                "Retiro",
                "L. de la Torre",
                "Belgrano C",
                "Nuñez",
                "Rivadavia",
                "Vte. Lopez",
                "Olivos",
                "La Lucila",
                "Martinez",
                "Acassuso",
                "San Isidro",
                "Beccar",
                "Victoria",
                "Virreyes",
                "San Fernando",
                "Carupa",
                "Tigre"
            };

            for (var i = 0; i < stationList.Count; i++)
            {
                var stationLocation = new Point(105, 130);
                stationLocation.X += (62*i);
                var tempStation = new Station
                {
                    Name = stationList[i],
                    BackwardTrack = null,
                    MapLocation = stationLocation
                };

                if (Stations.ElementAtOrDefault(i - 1) != null)
                {
                    tempStation.PreviousStation = Stations[i - 1];
                    Stations[i - 1].NextStation = tempStation;
                }

                Stations.Add(tempStation);
            }

            var depositMonitor = new Thread(DepositMonitor);
            depositMonitor.Start();

            var dispatcher = new Thread(Dispatcher);
            dispatcher.Start();
        }

        public void AddTrain(Train train)
        {
            lock (Locker)
            {
                _trainDeposit.Enqueue(train);
                Monitor.Pulse(Locker);
            }
            //_trainDeposit.Enqueue(train);
            //Stations[0].ForwardTrack = train;
            //train.CurrentStation = Stations[0];
            //train.Number = _trainDeposit.Count;
            //train.TrainName.Text = "Train #" + train.Number.ToString();

            //var thr = new Thread(train.Start);
            //thr.Start();
        }

        private void Dispatcher()
        {
            lock (Locker)
            {
                Monitor.Wait(Locker);
                var train = _trainDeposit.Dequeue();
                _activeTrains.Add(train);
                train.CurrentStation = Stations[0];
                train.Number = _activeTrains.Count;
                train.TrainName.Invoke(new Action(() => train.TrainName.Text = "Train #" + train.Number.ToString()));
                Stations[0].ForwardTrack = train;

                var thr = new Thread(train.Start);
                thr.Start();

                Monitor.Pulse(Locker);
                Dispatcher();
            }
        }

        private void DepositMonitor()
        {
            while (true)
            {
                lock (Locker)
                {
                    if (!_trainDeposit.Any() || Stations[0].ForwardTrack != null)
                    {
                        continue;
                    }
                    Monitor.Pulse(Locker);
                    Monitor.Wait(Locker);
                }
            }
        }

        public static void UpdateStatus(Train train)
        {
            train.LabelNumber.Invoke(new Action(() => train.LabelNumber.Text = train.Number.ToString()));
            train.LabelCurrentStation.Invoke(new Action(() => train.LabelCurrentStation.Text = train.CurrentStation.Name));
            train.LabelStatus.Invoke(new Action(() => train.LabelStatus.Text = train.Status));
            train.LabelCurrentCapacity.Invoke(new Action(() => train.LabelCurrentCapacity.Text = train.CurrentCapacity.ToString()));


            var destination = train.ForwardDirection
                ? train.CurrentStation?.NextStation?.Name
                : train.CurrentStation?.PreviousStation?.Name;

            switch (destination)
            {
                case null when train.ForwardDirection:
                    destination = train.CurrentStation.PreviousStation.Name;
                    break;
                case null when !train.ForwardDirection:
                    destination = train.CurrentStation.NextStation.Name;
                    break;
            }

            train.LabelDestination.Invoke(new Action(() => train.LabelDestination.Text = destination));
        }

        //private static void UpdateTrainsTable(object obj)
        //{
        //    var train = (Train) obj;
        //    train.LabelNumber.Invoke(new Action(() => train.LabelNumber.Text = train.Number.ToString()));
        //    train.LabelCurrentStation.Invoke(new Action(() => train.LabelCurrentStation.Text = train.CurrentStation.Name));
        //    train.LabelStatus.Invoke(new Action(() => train.LabelStatus.Text = train.Status));
        //    train.LabelCurrentCapacity.Invoke(new Action(() => train.LabelCurrentCapacity.Text = train.CurrentCapacity.ToString()));


        //    var destination = train.ForwardDirection
        //        ? train.CurrentStation?.NextStation?.Name
        //        : train.CurrentStation?.PreviousStation?.Name;

        //    switch (destination)
        //    {
        //        case null when train.ForwardDirection:
        //            destination = train.CurrentStation.PreviousStation.Name;
        //            break;
        //        case null when !train.ForwardDirection:
        //            destination = train.CurrentStation.NextStation.Name;
        //            break;
        //    }

        //    train.LabelDestination.Invoke(new Action(() => train.LabelDestination.Text = destination));
        //}

        public static void UpdateMap(Train train)
        {
            var lblPos = train.ForwardDirection
                ? new Point(train.CurrentStation.MapLocation.X, train.CurrentStation.MapLocation.Y)
                : new Point(train.CurrentStation.MapLocation.X, train.CurrentStation.MapLocation.Y + 50);
            train.TrainName.Invoke(new Action(() => train.TrainName.Visible = true));
            train.TrainName.Invoke(new Action(() => train.TrainName.Location = lblPos));
        }

        //private static void Update(object obj)
        //{
        //    var train = (Train) obj;
        //    var lblPos = train.ForwardDirection
        //        ? new Point(train.CurrentStation.MapLocation.X, train.CurrentStation.MapLocation.Y)
        //        : new Point(train.CurrentStation.MapLocation.X, train.CurrentStation.MapLocation.Y + 50);
        //    train.TrainName.Invoke(new Action(() => train.TrainName.Visible = true));
        //    train.TrainName.Invoke(new Action(() => train.TrainName.Location = lblPos));
        //}
    }
}
