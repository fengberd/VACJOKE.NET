using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NameChanger
{
    public partial class MainForm : Form
    {
        public static string[] defaultClantags = new string[]
        {
            "VACJOKE.NET",
            "unnamed"
        };

        public Process currentProcess = null;

        public int index = 0, width = 8;
        public List<string> frames = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            if(File.Exists("names.txt"))
            {
                try
                {
                    comboBox2.Items.Clear();
                    comboBox2.Items.AddRange(ParseNames(File.ReadAllText("names.txt",Encoding.UTF8)));
                }
                catch { }
            }
            if(comboBox2.Items.Count == 0)
            {
                comboBox2.Items.AddRange(defaultClantags);
            }
        }

        private void MainForm_Load(object sender,EventArgs e)
        {

        }

        private void timer1_Tick(object sender,EventArgs e)
        {
            var currentIds = comboBox1.Items.Cast<string>().Select(i => int.TryParse(i,out int id) ? id : -1).Where(i => i != -1);
            var latestIds = Process.GetProcessesByName("csgo").Select(i => i.Id);
            if(currentIds.Count() != latestIds.Count() || !latestIds.All(currentIds.Contains))
            {
                comboBox1.Enabled = latestIds.Count() != 0;
                comboBox1.Items.Clear();
                if(!comboBox1.Enabled)
                {
                    comboBox1.Items.Add("No process found.");
                }
                else
                {
                    comboBox1.Items.AddRange(latestIds.Select(i => (object)i.ToString()).ToArray());
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender,EventArgs e)
        {
            currentProcess = int.TryParse(comboBox1.Text,out int id) ? Process.GetProcessById(id) : null;
        }

        private void button1_Click(object sender,EventArgs e)
        {
            var form = new EditListForm(string.Join("\r\n",comboBox2.Items.Cast<string>().ToArray()));
            if(form.ShowDialog() == DialogResult.OK)
            {
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(ParseNames(form.textBox1.Text));
                if(comboBox2.Items.Count == 0)
                {
                    comboBox2.Items.AddRange(defaultClantags);
                }
                File.WriteAllText("names.txt",string.Join("\n",comboBox2.Items.Cast<string>().ToArray()),Encoding.UTF8);
            }
        }

        private void button2_Click(object sender,EventArgs e)
        {
            comboBox2.Text = comboBox2.Text.Replace("  "," ").TrimStart().TrimEnd();
            if(comboBox2.Text == "")
            {
                return;
            }
            var result = Utils.LoadData(currentProcess);
            if(result != null)
            {
                MessageBox.Show(result,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            result = Utils.SetName(currentProcess,comboBox2.Text);
            if(result != null)
            {
                MessageBox.Show(result,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        public object[] ParseNames(string raw)
        {
            return raw.Replace("\r","")
                .Split('\n')
                .Select(i => i.Replace("  "," ")
                    .TrimStart()
                    .TrimEnd())
                .Where(i => i != "")
                .Select(i => (object)i)
                .ToArray();
        }
    }
}
