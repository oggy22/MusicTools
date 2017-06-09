using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MusicalHearing
{
    public partial class Form1 : Form
    {
        const int WhiteKeyWidth = 40;
        const int BlackKeyWidth = 24;
        readonly int[] x = { 1, 2, 4, 5, 6 };

        public Form1()
        {
            List<Button> buttons = new List<Button>();

            //Black keys
            for (int i=0; i<3; i++)
            {
                for (int j=0; j<5; j++)
                {
                    Button button = new Button();
                    button.Size = new Size(BlackKeyWidth, 120);
                    button.Location = new Point(i * 7 * WhiteKeyWidth + WhiteKeyWidth * x[j] - BlackKeyWidth/2, 62);
                    button.BackColor = Color.Black;
                    button.ForeColor = Color.White;
                    button.MouseClick += Button_MouseClick;
                    this.Controls.Add(button);
                    buttons.Add(button);
                }
            }

            //White keys
            for (int i = 0; i < 7 * 3 + 1; i++)
            {
                Button button = new Button();
                button.Size = new Size(WhiteKeyWidth, 200);
                button.Location = new Point(WhiteKeyWidth * i, 62);
                button.BackColor = Color.White;
                button.MouseClick += Button_MouseClick;
                this.Controls.Add(button);
                buttons.Add(button);
            }
            buttons.Sort((b1, b2) => b1.Location.X - b2.Location.X);
            for (int i = 0; i < buttons.Count; i++)
                buttons[i].TabIndex = i;

            InitializeComponent();
        }

        static MidiOut midiOut = new MidiOut(0);

        private void Button_MouseClick(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button.Text == string.Empty)
            {
                midiOut.Send(MidiMessage.StartNote(48 + button.TabIndex, 100, 1).RawData);
                button.Text = "X";
            }
            else
            {
                midiOut.Send(MidiMessage.StartNote(48 + button.TabIndex, 0, 1).RawData);
                button.Text = string.Empty;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i=0; i<keys.Length; i++)
            {
                if (keys[i])
                    midiOut.Send(MidiMessage.StartNote(48 + i, 120, 1).RawData);
            }
        }

        static private int numberOfTones = 1;

        private bool[] keys = GenerateNotes(numberOfTones);

        static Random rand = new Random();

        static private bool[] GenerateNotes(int count)
        {
            bool[] notes = new bool[3 * 12 + 1];

            int start = rand.Next(15);
            notes[start] = true;
            for (int i = 1; i < count; i++)
            {
                start += 2 + rand.Next(4);
                notes[start] = true;
            }
            return notes;

        }

        static private bool[] GenerateNotes_old(int count)
        {
            bool[] notes = new bool[3 * 12 + 1];

            int? max = null;
            int? min = null;
            for (int i=0; i<count; i++)
            {
                int k;
                do
                {
                    k = rand.Next(notes.Length);
                } while (notes[k]);
                notes[k] = true;
                if (!min.HasValue || k < min)
                    min = k;
                if (!max.HasValue || k > min)
                    max = k;
            }

            if (max - min > 12 + 5 * (count - 2))
                return GenerateNotes(count);

            if (min > 20)
                return GenerateNotes(count);

            return notes;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            keys = GenerateNotes(numberOfTones);
            for (int i = 0; i < 12*3 + 1; i++)
                (Controls[i] as Button).Text = "";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i=0; i<keys.Length; i++)
            {
                FindButton(i).Text = keys[i] ? "*" : "";
            }
        }

        private Button FindButton(int tabindex)
        {
            for (int i = 0; true; i++)
                if (Controls[i].TabIndex == tabindex)
                    return Controls[i] as Button;
        }
    }
}
