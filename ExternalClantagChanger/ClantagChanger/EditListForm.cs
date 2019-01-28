using System;
using System.Windows.Forms;

namespace ClantagChanger
{
    public partial class EditListForm : Form
    {
        public EditListForm(string data)
        {
            InitializeComponent();
            textBox1.Text = data;
        }

        private void button1_Click(object sender,EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
