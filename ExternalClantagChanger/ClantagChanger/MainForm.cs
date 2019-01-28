using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ClantagChanger
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender,EventArgs e)
        {

        }

        private void button1_Click(object sender,EventArgs e)
        {
            var proc = Process.GetProcessesByName("csgo");
            if(proc.Length>0)
            {
                Utils.SetClantag(proc[0],"啪了你一下","啪了你一下");
            }
        }
    }
}
