using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    struct MelodyPartPointer
    {
        public MelodyPartList part;
        public int index;
        public int lastNoteIndex;

        public int MaxLength => lastNoteIndex + 1 - index;

        public IMelodyPart MelodyPart => part[index];
        public NoteWithDuration note => (NoteWithDuration)part[index];

        public void AssertCorrect()
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < part.Count);
        }

        public MelodyPartPointer(MelodyPartList part, int index)
        {
            this.part = part;
            this.index = index;
            Debug.Assert(part[index] is NoteWithDuration);

            lastNoteIndex = index+1;
            while (lastNoteIndex < part.Count && part[lastNoteIndex] is NoteWithDuration)
            {
                lastNoteIndex++;
            }

            lastNoteIndex--;

            Debug.Assert(part[lastNoteIndex] is NoteWithDuration);
        }

        public MelodyPartPointer(MelodyPartList part, int index, int lastNoteIndex)
        {
            this.part = part;
            this.index = index;
            Debug.Assert(part[index] is NoteWithDuration);

            this.lastNoteIndex = lastNoteIndex;
            Debug.Assert(part[lastNoteIndex] is NoteWithDuration);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MelodyPartPointer pnt))
                return false;

            return this == pnt;
        }

        /// <summary>
        /// Increments the pointer to the next element
        /// </summary>
        /// <returns>True if valid</returns>
        public bool Next()
        {
            index++;
            return index < part.Count;
        }

        public static bool operator ==(MelodyPartPointer p1, MelodyPartPointer p2)
        {
            return p1.part == p2.part && p1.index == p2.index;
        }

        public static bool operator !=(MelodyPartPointer p1, MelodyPartPointer p2)
        {
            return !p1.Equals(p2);
        }

        public override string ToString()
        {
            return $"index={index} part={part} MelodyPart={MelodyPart}";
        }

        public override int GetHashCode()
        {
            var hashCode = 1354351900;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MelodyPartList>.Default.GetHashCode(part);
            hashCode = hashCode * -1521134295 + index.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMelodyPart>.Default.GetHashCode(MelodyPart);
            hashCode = hashCode * -1521134295 + EqualityComparer<NoteWithDuration>.Default.GetHashCode(note);
            return hashCode;
        }
    }

    public static class ChomskyGrammarAnalysis
    {
        /// <summary>
        /// Perfomrs Chomsky analysis and reduces the list, similar to compression.
        /// </summary>
        /// <param name="melodyPartNodes">All nodes including input and all produced nodes</param>
        public static List<MelodyPartList> Reduce(List<MelodyPartList> melodyPartNodes)
        {
            List<MelodyPartList> allNodes = new List<MelodyPartList>(melodyPartNodes);

            int maxListCount = int.MaxValue;

            Swap swap = FindLongestSwap(melodyPartNodes);
            int count = 500;

            while (swap.list.Count > 2)
            {
                swap.AssertCorrect();

                // Assert the chunck size is not increasing
                Debug.Assert(swap.list.Count <= maxListCount);
                maxListCount = swap.list.Count;

                // If there is identical list already, don't add it, use the identical one
                MelodyPartList nodeExisting;
                if ((nodeExisting = allNodes.Find(node => node.IsIdentical(swap.list))) != null)
                    swap.list = nodeExisting;
                else
                    allNodes.Add(swap.list);

                //Assert that at most one part is whole
                if (swap.IsWhole1 && swap.IsWhole2)
                    Debug.Fail(message: "There are two identical parts. One should have referenced another instead of being created.");

                int offset1 = swap.pnt1.MelodyPart.GetNotes().First().note;
                swap.pnt1.part.RemoveRange(swap.pnt1.index, swap.list.Count);
                swap.pnt1.part.Insert(swap.pnt1.index, new MelodyPart(offset1, swap.list));

                if (!swap.IsWhole2)
                {
                    // If it's the same part, adjust the pnt2.index 
                    if (swap.pnt1.part == swap.pnt2.part)
                        swap.pnt2.index -= swap.list.Count - 1;

                    int offset2 = swap.pnt2.MelodyPart.GetNotes().First().note;
                    swap.pnt2.part.RemoveRange(swap.pnt2.index, swap.list.Count);
                    swap.pnt2.part.Insert(swap.pnt2.index, new MelodyPart(offset2, swap.list));
                }

                if (--count <= 0)
                    break;

                // Next swap
                swap = FindLongestSwap(allNodes);
            };

            foreach (var list in melodyPartNodes)
                Debug.Assert(list.RecursiveCount > 1);

            return allNodes;
        }

        [Conditional("DEBUG")]
        public static void Print(List<MelodyPartList> melodyPartLists)
        {
            Debug.WriteLine("ITERATION:");
            foreach (var mpl in melodyPartLists)
                Debug.WriteLine(mpl.ToString());

            Debug.WriteLine("");
        }

        public static void PostAnalysis(List<MelodyPartList> melodyPartLists)
        {
            foreach (MelodyPartList mpl in melodyPartLists)
            {
                HashSet<MelodyPartList> mpls = new HashSet<MelodyPartList>();
                UpdateLowerRecursive(mpl, mpls);

                foreach (var m in mpls)
                    m.TotalVoices++;
            }
        }

        private static void UpdateLowerRecursive(MelodyPartList mpl, HashSet<MelodyPartList> mplVoices)
        {
            mpl.TotalOccurances++;
            mplVoices.Add(mpl);

            foreach (var childNode in mpl.GetChildren())
                UpdateLowerRecursive(childNode, mplVoices);
        }

        /// <summary>
        /// Potential swap containing 2 pointers and the node/melody
        /// </summary>
        internal struct Swap
        {
            public MelodyPartPointer pnt1, pnt2;
            public MelodyPartList list;
            public int Length => list.Count;
            public bool IsWhole1 => pnt1.index == 0 && pnt1.part.Count == Length;
            public bool IsWhole2 => pnt2.index == 0 && pnt2.part.Count == Length;

            [Conditional("DEBUG")]
            public void AssertCorrect()
            {
                pnt1.AssertCorrect();
                pnt2.AssertCorrect();
                Debug.Assert(pnt1.index + list.Count <= pnt1.part.Count);
                Debug.Assert(pnt2.index + list.Count <= pnt2.part.Count);
                if (pnt1.part == pnt2.part)
                {
                    int i = pnt1.index;
                    int j = pnt2.index;
                    if (i < j)
                        Debug.Assert(i + list.Count <= j);
                    else
                        Debug.Assert(j + list.Count <= i);
                }

                if (list[0] is NoteWithDuration note0 &&
                    pnt1.part[pnt1.index+0] is NoteWithDuration p1note0 &&
                    pnt2.part[pnt2.index+0] is NoteWithDuration p2note0)
                    for (int i = 1; i < list.Count; i++)
                    {
                        if (list[i] is NoteWithDuration note &&
                        pnt1.part[pnt1.index + i] is NoteWithDuration p1note &&
                        pnt2.part[pnt2.index + i] is NoteWithDuration p2note)
                        {
                            Debug.Assert(-128 < note.note && note.note < 128);
                            Debug.Assert(p1note.note - p1note0.note == note.note - note0.note);
                            Debug.Assert(p2note.note - p2note0.note == note.note - note0.note);
                        }
                        else
                            Debug.Fail($"{list[i]} {pnt1.part[i]} {pnt2.part[i]} must be notes");
                    }
                else
                    Debug.Fail($"{list[0]} {pnt1.part[0]} {pnt2.part[0]} must be notes");
            }
        }

        internal static Swap FindLongestSwap(List<MelodyPartList> melodyPartLists)
        {
            Swap longestSwap = new Swap() { list = new MelodyPartList(MelodyPartList.Type.Melody) };

            foreach (MelodyPartPointer pntLeft in GetMelodyPartPointers(melodyPartLists))
            {
                NoteWithDuration note = (NoteWithDuration)pntLeft.MelodyPart;

                if (note.IsPause)
                    continue;

                if (pntLeft.MaxLength <= longestSwap.Length)
                    continue;

                bool valid = false;
                foreach (MelodyPartPointer pntRight in GetMelodyPartPointers(melodyPartLists))
                {
                    if (pntRight.MaxLength <= longestSwap.Length)
                        continue;

                    if (valid)
                    {
                        Debug.Assert(pntLeft.MelodyPart is NoteWithDuration);
                        Debug.Assert(pntLeft != pntRight);

                        Swap currentSwap = new Swap()
                        {
                            list = FindLongestMelodyPartList(pntLeft, pntRight),
                            pnt1 = pntLeft, pnt2 = pntRight
                        };

                        if (currentSwap.Length > 0)
                        {
                            currentSwap.AssertCorrect();

                            if (currentSwap.Length > longestSwap.Length)
                                longestSwap = currentSwap;
                        }
                    }
                    else if (pntLeft == pntRight)
                    {
                        valid = true;
                        continue;
                    }
                }
            }

            return longestSwap;
        }

        private static IEnumerable<MelodyPartPointer> GetMelodyPartPointers(List<MelodyPartList> melodyPartLists)
        {
            foreach (var list in melodyPartLists)
            {
                int i = 0;
                while (i < list.Count && !(list[i] is NoteWithDuration))
                {
                    i++;
                }

                if (i >= list.Count)
                    continue;

                MelodyPartPointer mpp = new MelodyPartPointer(list, i);
                int lastNote = mpp.lastNoteIndex;
                yield return mpp;

                for (i++; i < list.Count; i++)
                {
                    if (i <= lastNote)
                        yield return new MelodyPartPointer(list, i, lastNote);
                    else
                    {
                        while (i < list.Count && !(list[i] is NoteWithDuration))
                            i++;

                        if (i >= list.Count)
                            break;

                        mpp = new MelodyPartPointer(list, i);
                        lastNote = mpp.lastNoteIndex;
                        yield return mpp;
                    }
                }
            }
        }

        private static MelodyPartList FindLongestMelodyPartList(MelodyPartPointer pnt1, MelodyPartPointer pnt2)
        {
            MelodyPartList list = new MelodyPartList(MelodyPartList.Type.Melody);
            var pnt1Start = pnt1;
            var pnt2Start = pnt2;
            while (pnt1.MelodyPart is NoteWithDuration note1 && pnt2.MelodyPart is NoteWithDuration note2)
            {
                if (note1.Duration != note2.Duration)
                    break;

                // Add to the list
                if (list.Count == 0)
                    list.Add(new NoteWithDuration(0, note1.Duration));
                else
                {
                    if (note1.IsPause != pnt1Start.note.IsPause)
                        break;

                    if (note2.IsPause != pnt2Start.note.IsPause)
                        break;

                    int newNote1 = note1.note - pnt1Start.note.note;
                    int newNote2 = note2.note - pnt2Start.note.note;
                    if (newNote1 != newNote2)
                        break;

                    list.Add(new NoteWithDuration(newNote1, pnt1.MelodyPart.duration));
                }

                // Increment pointers
                if (!pnt1.Next() || !pnt2.Next())
                    break;

                if (pnt1 == pnt2Start)
                    break;
            }
            pnt1 = pnt1Start;
            pnt2 = pnt2Start;

#if DEBUG
            if (list.Count > 0)
            {
                Swap swap = new Swap()
                {
                    list = list,
                    pnt1 = pnt1,
                    pnt2 = pnt2
                };
                swap.AssertCorrect();
            }
#endif

            return list;
        }
    }
}