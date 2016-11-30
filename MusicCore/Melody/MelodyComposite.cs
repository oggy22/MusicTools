using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public class MelodyComposite : MelodyBase
    {
        struct Component
        {
            public MelodyBase melody;
            public int alteration;
        }

        List<Component> components;

        public override Fraction StartPause => components[0].melody.StartPause;

        public override IEnumerable<NoteWithDuration> Anacrusis => components[0].melody.Anacrusis.Select(nwd => new NoteWithDuration()
        {
            note = nwd.note + components[0].alteration,
            duration = nwd.duration
        });

        public override Fraction Duration
        {
            get
            {
                Fraction sum = new Fraction(0, 0);
                foreach (var comp in components)
                    sum = sum + comp.melody.Duration;
                return sum;
            }
        }

        public override IEnumerable<Tuple<Note, Fraction>> NotesOld()
        {
            foreach (var component in components)
                foreach (var tuple in component.melody.NotesOld())
                {
                    Note note = tuple.Item1;
                    note.note += component.alteration;
                    yield return new Tuple<Note, Fraction>(note, tuple.Item2);
                }
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            List<NoteWithDuration> current = new List<NoteWithDuration>();
            foreach (var component in components)
            {
                Fraction fractSum = component.melody.Duration - component.melody.StartPause;
                foreach (var nwd in component.melody.Anacrusis)
                    fractSum = fractSum - nwd.duration;

                Debug.Assert(fractSum.p > 0);
                foreach (var nwd in current)
                {
                    fractSum = fractSum - nwd.duration;
                    if (fractSum.p < 0)
                    {
                        Fraction dur = nwd.duration + fractSum;
                        Debug.Assert(dur.p > 0);
                        yield return new NoteWithDuration() { note = nwd.note, duration = dur };
                        break;
                    }
                    yield return nwd;
                    if (fractSum.p == 0)
                        break;
                }
                foreach (var nwd in component.melody.Anacrusis)
                    yield return nwd;

                current = new List<NoteWithDuration>();

                if (component.melody.StartPause.p != 0)
                {
                    yield return NoteWithDuration.MakePause(component.melody.StartPause);
                }

                foreach (var nwd in component.melody.Notes())
                {
                    current.Add(new NoteWithDuration()
                    {
                        note = nwd.note + component.alteration,
                        duration = nwd.duration
                    });
                }
            }

            foreach (var nwd in current)
            {
                yield return nwd;
            }
        }

        public MelodyComposite(string st, params MelodyBase[] melodies)
        {
            Debug.Assert(st.Length >= 2, "At least two-fold");
            components = new List<Component>();
            char min = st.Where(c => char.IsLetter(c)).Min();
            for (int i = 0; i < st.Length; i++)
            {
                if (char.IsLetter(st[i]))
                {
                    Component component = new Component() { alteration = 0, melody = melodies[st[i] - min] };
                    while (i + 1 < st.Length && !char.IsLetter(st[i + 1]))
                    {
                        switch (st[i + 1])
                        {
                            case '+': component.alteration++; break;
                            case '-': component.alteration--; break;
                            default: throw new ArgumentException($"Unexpected symbol {st[i + 1]}");
                        }
                        i++;
                    }
                    components.Add(component);
                    continue;
                }
                throw new ArgumentException($"Unexpected symbol {st[i]}");
            }
        }
    }
}
