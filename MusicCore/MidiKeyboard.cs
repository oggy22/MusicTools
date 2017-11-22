using NAudio.Midi;
using System;

namespace MusicCore
{
    public class MidiKeyboard
    {
        static MidiOut midiOut = new MidiOut(0);
        public readonly ToneSet toneset;
        public event Action<int> toneDown;
        public event Action<int> toneUp;
        public bool PlayNotes { get; private set; }
        public MidiKeyboard(bool playNotes = true)
        {
            PlayNotes = playNotes;
            toneset = new ToneSet();
            MidiIn midiin = new MidiIn(0);
            midiin.Start();
            midiin.MessageReceived += Midiin_MessageReceived;
        }

        private void Midiin_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn)
            {
                var noteonevent = e.MidiEvent as NoteOnEvent;
                if (PlayNotes)
                    midiOut.Send(MidiMessage.StartNote(noteonevent.NoteNumber, 100, 1).RawData);

                toneset.Add(noteonevent.NoteNumber);
                toneDown(noteonevent.NoteNumber);
            }
            else if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteevent = e.MidiEvent as NoteEvent;
                if (PlayNotes)
                    midiOut.Send(MidiMessage.StopNote(noteevent.NoteNumber, 100, 1).RawData);

                toneset.Remove(noteevent.NoteNumber);
                toneUp(noteevent.NoteNumber);
            }
        }
    }
}
