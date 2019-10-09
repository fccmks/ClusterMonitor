using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClusterMonitoring
{
    public partial class MainForm : Form
    {
        private readonly SFConnector _sfc = new SFConnector();
        private string _pathToSettings = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            timer1.Interval = 10000;
        }

        private void btnRefresh_Click(object sender, System.EventArgs e)
        {
            if (_pathToSettings.Length > 0)
            {
                DoWork();
            }
            else
            {
                MessageBox.Show(@"Choose settings file first!","No settings");
            }
        }

        private void btnSettings_Click(object sender, System.EventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = @"Settings|*.json"};
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _pathToSettings = openFileDialog.FileName;
                timer1.Enabled = true;
                DoWork();
            }
        }

        private void DoWork()
        {
            if (backgroundWorker1.IsBusy)
                return;
            
            btnRefresh.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            _sfc.FillTable(_pathToSettings);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender,
            System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            DrawListView(_sfc.Clusters);
            btnRefresh.Enabled = true;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            DoWork();
        }

        private void DrawListView(IEnumerable<ClusterInfo> list)
        {
            var data = list?.Select(x =>
            {
                var item = new ListViewItem(x.ClusterName);
                item.SubItems.Add(x.ConnectionEndpoint);
                item.SubItems.Add(x.CodeVersion);
                return item;
            }).ToArray() ?? new ListViewItem[0];

            listView.Items.Clear();
            listView.Items.AddRange(data);
        }
    }
}
