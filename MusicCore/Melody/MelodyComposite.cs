using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicCore
{
    public class MelodyComposite : MelodyBase
    {
        struct Component
        {
            public MelodyBase melody;
            public int[] alterations;
        }

        List<Component> components;

        public override Fraction StartPause => components[0].melody.StartPause;

        public override IEnumerable<NoteWithDuration> Anacrusis => components[0].melody.Anacrusis.Select(
            nwd => new NoteWithDuration(nwd.note + components[0].alterations[0], nwd.duration));

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

        public override IEnumerable<NoteWithDuration> Notes()
        {
            List<NoteWithDuration> current = new List<NoteWithDuration>();
            foreach (var component in components)
            {
                Fraction fractSum = component.melody.Duration;
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
                        yield return new NoteWithDuration(nwd.note, dur);
                        break;
                    }
                    yield return nwd;
                    if (fractSum.p == 0)
                        break;
                }
                foreach (var nwd in component.melody.Anacrusis)
                    yield return new NoteWithDuration(nwd.note + component.alterations[0], nwd.duration);

                current = new List<NoteWithDuration>();

                if (component.melody.StartPause.p != 0)
                    current.Add(NoteWithDuration.MakePause(component.melody.StartPause));

                foreach (var nwd in component.melody.Notes(component.alterations))
                {
                    current.Add(new NoteWithDuration(nwd.note, nwd.duration));
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
                    Component component = new Component() { alterations = new int[]{ 0 }, melody = melodies[st[i] - min] };
                    while (i + 1 < st.Length && !char.IsLetter(st[i + 1]))
                    {
                        switch (st[i + 1])
                        {
                            case '+': component.alterations[0]++; break;
                            case '-': component.alterations[0]--; break;
                            case '(':
                                {
                                    int j = i + 2;
                                    do
                                    {
                                        if (j >= st.Length)
                                            throw new ArgumentException("Unexpected end of string, expecting ')'");
                                        j++;
                                    } while (st[j] != ')');
                                    component.alterations =
                                        st.Substring(i + 2, j - 1 - (i + 1)).Split(',')
                                        .Select(s => int.Parse(s)).ToArray();
                                    i = j;
                                    continue;
                                }
                        }
                        i++;
                    }
                    components.Add(component);
                    continue;
                }
                throw new ArgumentException($"Unexpected symbol {st[i]}");
            }
        }

        //private MelodyBase FindMelodyBase(char c)
        //{

        //}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<MelodyBase, char> dict = new Dictionary<MelodyBase, char>();
            dict[components[0].melody] = 'A';

            foreach (var component in components)
            {
                if (!dict.ContainsKey(component.melody))
                    dict.Add(component.melody, (char)(dict.Values.Max() + (char)(1)));

                sb.Append(dict[component.melody]);
                int alt = component.alterations[0];
                sb.Append(new string(alt > 0 ? '+' : '-', Math.Abs(alt)));
            }
            return sb.ToString();
        }
    }
}
