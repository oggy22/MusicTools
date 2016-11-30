using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalHearing
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //static MidiOut midiOut = new MidiOut(0);

        public static string PlaySounds()
        {
            //MidiOut.Volume = 65535;
            Random rand = new Random();
            int tone = 60;
            string stRet = "";
            do
            {
                //midiOut.Send(MidiMessage.StartNote(tone, 100, 1).RawData);
                stRet += tone + " ";
                Thread.Sleep(10);
                tone += 2 + rand.Next() % 3;
            } while (tone <= 70);
            Thread.Sleep(10);
            //midiOut.Send(MidiMessage.StartNote(65, 100, 1).RawData);
            Thread.Sleep(1000);
            return stRet;
        }
    }
}
