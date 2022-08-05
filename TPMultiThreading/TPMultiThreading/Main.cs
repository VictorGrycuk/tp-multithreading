using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPMultiThreading
{
    public partial class Main : Form
    {
        private Central _central;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _central = new Central();
        }

        private void BtnAddTrain_Click(object sender, EventArgs e)
        {
            var newTrain = new Train
            {
                Speed = (int)numSpeed.Value,
                BaseWaitingTime = (int)numBaseWaitingTime.Value,
                TrainName = GetMapLabel(),
                LabelNumber = new Label { AutoSize = true },
                LabelCurrentStation = new Label { AutoSize = true },
                LabelDestination = new Label { AutoSize = true },
                LabelStatus = new Label { AutoSize = true },
                LabelCurrentCapacity = new Label { AutoSize = true }
            };

            tableActiveTrains.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableActiveTrains.Controls.Add(newTrain.LabelNumber);
            tableActiveTrains.Controls.Add(newTrain.LabelCurrentStation);
            tableActiveTrains.Controls.Add(newTrain.LabelDestination);
            tableActiveTrains.Controls.Add(newTrain.LabelStatus);
            tableActiveTrains.Controls.Add(newTrain.LabelCurrentCapacity);
            tableActiveTrains.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _central.AddTrain(newTrain);
        }

        private Label GetMapLabel()
        {
            return new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Parent = picMap,
                Visible = false
            };
        }
    }
}
