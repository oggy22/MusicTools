using NAudio.Midi;
using System;
using System.IO;

namespace MusicCore
{
    public static class CSVReader
    {
        public static Composition Read(string filename, Fraction? prec = null)
        {
            var reader = new StreamReader(filename);

            MelodyPartList list = new MelodyPartList(MelodyPartList.Type.Voice);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == "") continue;
                var values = line.Split(',');
                if (values.Length != 2)
                    throw new Exception($"The line `{line}` must contain exactly two values.");

                int note = int.Parse(values[0].Trim());
                Fraction duration = Fraction.Parse(values[1].Trim());
                NoteWithDuration nwd = new NoteWithDuration(60 + note, duration);
                list.Add(nwd);
            }

            Composition composition = new Composition();
            composition.millisecondsPerNote = 500; // todo: arbitrary tempo
            composition.AddVoice(list);
            return composition;
        }
    }
}