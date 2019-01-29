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
            "[VALVᴱ]"
        };

        public Process currentProcess = null;

        public int index = 0, width = 8;
        public List<string> frames = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            if(File.Exists("clantags.txt"))
            {
                try
                {
                    comboBox2.Items.Clear();
                    comboBox2.Items.AddRange(ParseClantags(File.ReadAllText("clantags.txt",Encoding.UTF8)));
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
            comboBox3.SelectedIndex = comboBox4.SelectedIndex = 0;
        }

        private void timer1_Tick(object sender,EventArgs e)
        {
            var currentIds = comboBox1.Items.Cast<string>().Select(i => int.TryParse(i,out int id) ? id : -1).Where(i => i != -1);
            var latestIds = Process.GetProcessesByName("csgo").Select(i => i.Id);
            if(currentIds.Count() != latestIds.Count() || !latestIds.All(currentIds.Contains))
            {
                checkBox1.Checked = false;
                comboBox1.Enabled = latestIds.Count() != 0;
                comboBox1.Items.Clear();
                if(!comboBox1.Enabled)
                {
                    comboBox1.Items.Add("No process found.");
                    checkBox1.Checked = false;
                }
                else
                {
                    comboBox1.Items.AddRange(latestIds.Select(i => (object)i.ToString()).ToArray());
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        private void timer2_Tick(object sender,EventArgs e)
        {
            if(frames.Count == 0)
            {
                return;
            }
            var result = Utils.SetClantag(currentProcess,frames[index],frames[index]);
            if(result != null)
            {
                MessageBox.Show(result,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            index += comboBox4.SelectedIndex == 1 ? -1 : 1;
            if(index >= frames.Count)
            {
                index = 0;
            }
            if(index < 0)
            {
                index = frames.Count - 1;
            }
        }

        private void checkBox1_CheckedChanged(object sender,EventArgs e)
        {
            BuildFrames();
            timer2.Enabled = checkBox1.Checked;
            comboBox2.Enabled = !checkBox1.Checked;
        }

        private void textBox1_Leave(object sender,EventArgs e)
        {
            if(int.TryParse(textBox1.Text.Trim(),out int time) && time >= 20)
            {
                timer2.Interval = time;
            }
            textBox1.Text = timer2.Interval.ToString();
        }

        private void textBox2_Leave(object sender,EventArgs e)
        {
            if(int.TryParse(textBox2.Text.Trim(),out int w) && w >= 2 && w <= 15)
            {
                width = w;
            }
            textBox2.Text = width.ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender,EventArgs e)
        {
            currentProcess = int.TryParse(comboBox1.Text,out int id) ? Process.GetProcessById(id) : null;
        }

        private void comboBox3_SelectedIndexChanged(object sender,EventArgs e)
        {
            BuildFrames();
        }

        private void button1_Click(object sender,EventArgs e)
        {
            var form = new EditListForm(string.Join("\r\n",comboBox2.Items.Cast<string>().ToArray()));
            if(form.ShowDialog() == DialogResult.OK)
            {
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(ParseClantags(form.textBox1.Text));
                if(comboBox2.Items.Count == 0)
                {
                    comboBox2.Items.AddRange(defaultClantags);
                }
                File.WriteAllText("clantags.txt",string.Join("\n",comboBox2.Items.Cast<string>().ToArray()),Encoding.UTF8);
            }
        }

        private void button2_Click(object sender,EventArgs e)
        {
            comboBox2.Text = comboBox2.Text.Replace("  "," ").TrimStart().TrimEnd();
            if(comboBox2.Text == "")
            {
                return;
            }
            var result = Utils.SetClantag(currentProcess,comboBox2.Text,comboBox2.Text);
            if(result != null)
            {
                MessageBox.Show(result,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        public object[] ParseClantags(string raw)
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

        public void BuildFrames()
        {
            var tag = comboBox2.Text = comboBox2.Text.Replace("  "," ").TrimStart().TrimEnd() + " ";
            frames.Clear();
            switch(comboBox3.SelectedIndex)
            {
            case 0: // Scroll
                if(width >= tag.Length)
                {
                    for(int i = 0;i < tag.Length;i++)
                    {
                        frames.Add(tag.Substring(i) + tag.Substring(0,i));
                    }
                    frames.RemoveAt(0);
                }
                else
                {
                    for(int i = 0;i < tag.Length;i++)
                    {
                        if(tag.Length - i < width)
                        {
                            frames.Add(tag.Substring(i) + tag.Substring(0,width - (tag.Length - i)));
                        }
                        else
                        {
                            frames.Add(tag.Substring(i,width));
                        }
                    }
                }
                break;
            case 1: // Spell
                for(int i = 0;i < tag.Length;i++)
                {
                    if(i % width == 0)
                    {
                        frames.Add("");
                    }
                    frames.Add(tag.Substring((int)Math.Floor((double)i / width) * width,i % width + 1));
                }
                break;
            case 2: // Process
                for(int i = 0;i < tag.Length;i++)
                {
                    if(i % width == 0)
                    {
                        frames.Add(new string('.',Math.Min(width,tag.Length)));
                    }
                    frames.Add(tag.Substring((int)Math.Floor((double)i / width) * width,i % width + 1).PadRight(Math.Min(width,tag.Length),'.'));
                }
                break;
            case 3: // Switch All
                frames.AddRange(comboBox2.Items.Cast<object>().Select(i => i.ToString()));
                break;
            }
        }
    }
}
