using Locker_v1.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Locker_v1
{
    public partial class Shortcut : Form
    {
        private Queue<string> combination;
        private int length = 1;

        public Shortcut()
        {
            InitializeComponent();
            combination = new Queue<string>();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            lblCombination.Text = "Null";
            combination.Clear();
            length = Convert.ToInt32((sender as RadioButton).Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lblCombination.Text = "Null";
            combination.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(lblCombination.Text != "Null")
            {
                Settings.Default.Combination = lblCombination.Text;
                Settings.Default.Save();
                this.Close();
            }
        }

        private void Shortcut_KeyDown(object sender, KeyEventArgs e)
        {
            if (!combination.Contains(e.KeyCode.ToString()))
            {
                combination.Enqueue(e.KeyCode.ToString());
            }

            if (combination.Count > length)
            {
                combination.Dequeue();
            }

            lblCombination.Text = "";

            string[] values = combination.ToArray();

            for (int i = 0; i < values.Length; i++)
            {
                lblCombination.Text += values[i];

                if (i + 1 < values.Length)
                {
                    lblCombination.Text += "+";
                }
            }
        }
    }
}
