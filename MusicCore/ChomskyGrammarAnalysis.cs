using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    struct MelodyPartPointer
    {
        public MelodyPartList part;
        public int firstNoteIndex;
        public int lastNoteIndex;

        public int MaxLength => lastNoteIndex + 1 - firstNoteIndex;

        public IMelodyPart MelodyPart => part[firstNoteIndex];
        public NoteWithDuration firstNote => (NoteWithDuration)part[firstNoteIndex];

        public void AssertCorrect()
        {
            Debug.Assert(firstNoteIndex >= 0);
            Debug.Assert(firstNoteIndex < part.Count);
        }

        /// <summary>
        /// lastNoteIndex as far as possible
        /// </summary>
        public MelodyPartPointer(MelodyPartList part, int index)
        {
            this.part = part;
            this.firstNoteIndex = index;
            Debug.Assert(part[index] is NoteWithDuration);

            lastNoteIndex = index + 1;
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
            this.firstNoteIndex = index;
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
            firstNoteIndex++;
            return firstNoteIndex < part.Count;
        }

        public static bool operator ==(MelodyPartPointer p1, MelodyPartPointer p2)
        {
            return p1.part == p2.part && p1.firstNoteIndex == p2.firstNoteIndex;
        }

        public static bool operator !=(MelodyPartPointer p1, MelodyPartPointer p2)
        {
            return !p1.Equals(p2);
        }

        public override string ToString()
        {
            return $"pointsTo={(NoteWithDuration)part[firstNoteIndex]} index={firstNoteIndex} part={part} MelodyPart={MelodyPart}";
        }

        public override int GetHashCode()
        {
            var hashCode = 1354351900;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MelodyPartList>.Default.GetHashCode(part);
            hashCode = hashCode * -1521134295 + firstNoteIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMelodyPart>.Default.GetHashCode(MelodyPart);
            hashCode = hashCode * -1521134295 + EqualityComparer<NoteWithDuration>.Default.GetHashCode(firstNote);
            return hashCode;
        }
    }

    public static class ChomskyGrammarAnalysis
    {
        /// <summary>
        /// Produces a list containing all similar scales.
        /// For example, if given only major scale (or similarly only minor scale)
        /// it will return: ionian(major), dorian, phrygian, lydian, mixolydian, aeolian and locrian scale.
        /// </summary>
        private static List<TwelveToneSet> CalculateAllScales(List<TwelveToneSet> scales)
        {
            if (scales == null)
                return null;

            // Assert each scale is rooted, no two scales are similar
            for (int i = 0; i < scales.Count; i++)
            {
                Debug.Assert(scales[i].IsRooted);

                for (int j = i + 1; j < scales.Count; j++)
                    Debug.Assert(!scales[i].IsSimilar(scales[j]));
            }

            List<TwelveToneSet> allScales = new List<TwelveToneSet>();
            foreach (var scale in scales)
            {
                TwelveToneSet currScale = scale;
                do
                {
                    // Shift to next inversion e.g. ionian to dorian
                    do
                    {
                        currScale = currScale.ShiftRight(1);
                    } while (!currScale.IsRooted);

                    allScales.Add(currScale);
                } while (currScale != scale);
            }

            return allScales;
        }

        /// <summary>
        /// Perfomrs Chomsky analysis and reduces list elements, extracting equal parts into newly created elements.
        /// The method is similar to compression.
        /// </summary>
        /// <param name="melodyPartNodes">All nodes including input and all produced nodes</param>
        /// <param name="scales">
        /// Scales for diatonic option. Within the list no scale should be similar to any other.
        /// Two scales are similar if one is inversion of another one,
        /// for example major and minor scale,
        /// but minor haromnic, minor melodic and minor natural are not similar to each other.
        /// If null, no diatonic option.
        /// </param>
        public static List<MelodyPartList> Reduce(IEnumerable<MelodyPartList> melodyPartNodes, List<TwelveToneSet> scales = null)
        {
            List<MelodyPartList> allNodes = new List<MelodyPartList>(melodyPartNodes);

            int maxListCount = int.MaxValue;

            List<TwelveToneSet> allScales = CalculateAllScales(scales);

            Swap swap = FindLongestSwap(melodyPartNodes, allScales);
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
                if (swap.IsWhole1 && swap.IsWhole2
                    && swap.pnt1.part.type != MelodyPartList.Type.Voice
                    && swap.pnt2.part.type != MelodyPartList.Type.Voice)
                    Debug.Fail(message: "There are two identical parts. One should have referenced another instead of being created.");

                int offset1 = swap.pnt1.MelodyPart.GetNotes().First().note;
                swap.pnt1.part.RemoveRange(swap.pnt1.firstNoteIndex, swap.list.Count);
                MelodyPart mp1 = swap.IsDToC ?
                    new MelodyPartDtoC(offset1, swap.scales1.First(), swap.list) :
                    new MelodyPart(offset1, swap.list);
                swap.pnt1.part.Insert(swap.pnt1.firstNoteIndex, mp1);

                if (!swap.IsWhole2 || swap.IsDToC)
                {                    
                    // If it's the same part, adjust the pnt2.index 
                    if (swap.pnt1.part == swap.pnt2.part)
                        swap.pnt2.firstNoteIndex -= swap.list.Count - 1;

                    int offset2 = swap.pnt2.MelodyPart.GetNotes().First().note;
                    swap.pnt2.part.RemoveRange(swap.pnt2.firstNoteIndex, swap.list.Count);
                    MelodyPart mp2 = swap.IsDToC ?
                        new MelodyPartDtoC(offset2, swap.scales2.First(), swap.list) :
                        new MelodyPart(offset2, swap.list);

                    swap.pnt2.part.Insert(swap.pnt2.firstNoteIndex, mp2);
                }
                else
                {
                    // This is unexpected, but may be possible.
                    // If it happens rethink it.
                    Debug.Assert(!swap.IsDToC);
                }

                if (--count <= 0)
                    break;

                // Next swap
                swap = FindLongestSwap(allNodes, allScales);
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

        public static void PostAnalysis(IEnumerable<MelodyPartList> melodyPartLists)
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
        internal class Swap
        {
            public MelodyPartPointer pnt1, pnt2;
            public HashSet<TwelveToneSet> scales1, scales2;
            public MelodyPartList list;
            public int Length => list.Count;
            public bool IsWhole1 => pnt1.firstNoteIndex == 0 && pnt1.part.Count == Length;
            public bool IsWhole2 => pnt2.firstNoteIndex == 0 && pnt2.part.Count == Length;

            public bool IsDToC => list.IsDiatonic && !pnt1.part.IsDiatonic;

            [Conditional("DEBUG")]
            public void AssertCorrect()
            {
                Debug.Assert(pnt1.part.IsDiatonic == pnt2.part.IsDiatonic);
                //Debug.Assert(!pnt1.part.IsDiatonic || list.IsDiatonic);

                pnt1.AssertCorrect();
                pnt2.AssertCorrect();
                Debug.Assert(pnt1.firstNoteIndex + list.Count <= pnt1.part.Count);
                Debug.Assert(pnt2.firstNoteIndex + list.Count <= pnt2.part.Count);
                if (pnt1.part == pnt2.part)
                {
                    int i = pnt1.firstNoteIndex;
                    int j = pnt2.firstNoteIndex;
                    if (i < j)
                        Debug.Assert(i + list.Count <= j);
                    else
                        Debug.Assert(j + list.Count <= i);
                }

                if (list[0] is NoteWithDuration note0 &&
                    pnt1.part[pnt1.firstNoteIndex + 0] is NoteWithDuration p1note0 &&
                    pnt2.part[pnt2.firstNoteIndex + 0] is NoteWithDuration p2note0)
                    for (int i = 1; i < list.Count; i++)
                    {
                        if (list[i] is NoteWithDuration note &&
                        pnt1.part[pnt1.firstNoteIndex + i] is NoteWithDuration p1note &&
                        pnt2.part[pnt2.firstNoteIndex + i] is NoteWithDuration p2note)
                        {
                            if (!note.IsPause)
                            {
                                Debug.Assert(-128 < note.note && note.note < 128);
                                if (this.IsDToC)
                                {
                                    Debug.Assert(
                                        scales1.First().ChromaticToDiatonic(p1note.note - p1note0.note)
                                        == note.note - note0.note);
                                    Debug.Assert(
                                        scales2.First().ChromaticToDiatonic(p2note.note - p2note0.note)
                                        == note.note - note0.note);
                                }
                                else
                                {
                                    Debug.Assert(p1note.note - p1note0.note == note.note - note0.note);
                                    Debug.Assert(p2note.note - p2note0.note == note.note - note0.note);
                                }
                            }
                        }
                        else
                            Debug.Fail($"{list[i]} {pnt1.part[i]} {pnt2.part[i]} must be notes");
                    }
                else
                    Debug.Fail($"{list[0]} {pnt1.part[0]} {pnt2.part[0]} must be notes");
            }
        }

        internal static Swap FindLongestSwap(IEnumerable<MelodyPartList> melodyPartLists, List<TwelveToneSet> allScales)
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

                    if (pntRight.part.IsDiatonic != pntLeft.part.IsDiatonic)
                        continue;

                    if (valid)
                    {
                        Debug.Assert(pntLeft.MelodyPart is NoteWithDuration);
                        Debug.Assert(pntLeft != pntRight);

                        Swap currentSwap = FindLongestSwap(pntLeft, pntRight, allScales);

                        if (currentSwap?.Length > 0)
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

            Debug.Assert(longestSwap.pnt1.part.IsDiatonic == longestSwap.pnt2.part.IsDiatonic);

            if (longestSwap.pnt1.part.IsDiatonic)
                longestSwap.list.IsDiatonic = true;

            return longestSwap;
        }

        private static IEnumerable<MelodyPartPointer> GetMelodyPartPointers(IEnumerable<MelodyPartList> melodyPartLists)
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

                        if (!((NoteWithDuration)list[i]).IsPause)
                        {
                            mpp = new MelodyPartPointer(list, i);
                            lastNote = mpp.lastNoteIndex;
                            yield return mpp;
                        }
                    }
                }
            }
        }

        private static Swap FindLongestSwap(MelodyPartPointer pnt1, MelodyPartPointer pnt2, List<TwelveToneSet> allScales)
        {
            MelodyPartList list = new MelodyPartList(MelodyPartList.Type.Melody);
            var pnt1Start = pnt1;
            var pnt2Start = pnt2;
            Debug.Assert(pnt1.part.IsDiatonic == pnt2.part.IsDiatonic);
            while (pnt1.MelodyPart is NoteWithDuration note1 && pnt2.MelodyPart is NoteWithDuration note2)
            {
                if (note1.Duration != note2.Duration)
                    break;

                if (note1.IsPause != note2.IsPause)
                    break;

                // Add to the list
                if (list.Count == 0)
                    list.Add(new NoteWithDuration(0, note1.Duration));
                else
                {
                    if (note1.IsPause)
                    {
                        list.Add(new NoteWithDuration(pnt1.MelodyPart.duration));
                    }
                    else
                    {
                        Debug.Assert(!pnt1Start.firstNote.IsPause);
                        Debug.Assert(!pnt2Start.firstNote.IsPause);
                        int newNote1 = note1.note - pnt1Start.firstNote.note;
                        int newNote2 = note2.note - pnt2Start.firstNote.note;
                        if (newNote1 != newNote2)
                        {
                            var diatSwap = TryDiatonic(pnt1, pnt2, allScales, newNote1, newNote2, pnt1.MelodyPart.duration, list, pnt1Start, pnt2Start);

                            if (diatSwap != null)
                                return diatSwap;

                            break;
                        }

                        list.Add(new NoteWithDuration(newNote1, pnt1.MelodyPart.duration));
                    }
                }

                // Increment pointers
                if (!pnt1.Next() || !pnt2.Next())
                    break;

                if (pnt1 == pnt2Start)
                    break;
            }
            pnt1 = pnt1Start;
            pnt2 = pnt2Start;

            if (list.Count > 0)
            {
                Swap swap = new Swap()
                {
                    list = list,
                    pnt1 = pnt1,
                    pnt2 = pnt2
                };
#if DEBUG
                swap.AssertCorrect();
#endif
                return swap;
            }

            return null;
        }

        private static Swap TryDiatonic(
            MelodyPartPointer pnt1,
            MelodyPartPointer pnt2,
            List<TwelveToneSet> allScales,
            int newNote1,
            int newNote2,
            Fraction newNoteDuration,
            MelodyPartList list,
            MelodyPartPointer pnt1Start,
            MelodyPartPointer pnt2Start)
        {
            // Diatonic option on?
            if (allScales == null)
                return null;

            // Already diatonic?
            if (pnt1.part.IsDiatonic)
                return null;

            // Conflicting tones too far off?
            if (Math.Abs(newNote1 - newNote2) > 2) // maybe > 1
                return null;

            // First toneset
            HashSet<TwelveToneSet> allScales1 = AllScales(list.GetNotes().Select(nwd => nwd.note), newNote1, allScales);
            if (allScales1.Count == 0)
                return null;

            // Second toneset
            HashSet<TwelveToneSet> allScales2 = AllScales(list.GetNotes().Select(nwd => nwd.note), newNote2, allScales);
            if (allScales2.Count == 0)
                return null;

            var setNote1 = new HashSet<int>(allScales1.Select(ttw => ttw.ChromaticToDiatonic(newNote1)));
            var setNote2 = new HashSet<int>(allScales2.Select(ttw => ttw.ChromaticToDiatonic(newNote2)));

            var intersect = setNote1.Intersect(setNote2);

            // The two sets intersect at most 1 element
            Debug.Assert(intersect.Count() <= 1);

            if (intersect.Count() == 0)
                return null;

            // Diatonic position of the new note
            int diatonicTone = intersect.First();

            var allScales1p = allScales1.Where(tts => tts.ChromaticToDiatonic(newNote1) == diatonicTone).ToHashSet();
            Debug.Assert(allScales1p.Count() > 0);
            allScales1 = allScales1p;

            allScales2 = allScales2.Where(tts => tts.ChromaticToDiatonic(newNote2) == diatonicTone).ToHashSet();
            Debug.Assert(allScales2.Count() > 0);

            // Convert into diatonic List
            MelodyPartList diatonicList = new MelodyPartList(MelodyPartList.Type.Melody, true);
            HashSet<TwelveToneSet> unionScales = allScales1.Union(allScales2).ToHashSet();
            NoteWithDuration note;
            foreach (var t in list.GetNotes())
            {
                note = t.IsPause ? new NoteWithDuration(t.Duration) : new NoteWithDuration(
                    allScales1.First().ChromaticToDiatonic(t.note),
                    t.Duration);
                diatonicList.Add(note);

                if (!t.IsPause)
                foreach (var scale in unionScales)
                    Debug.Assert(note.note == scale.ChromaticToDiatonic(t.note));
            }

            // Add new note
            note = new NoteWithDuration(
                allScales1.First().ChromaticToDiatonic(newNote1),
                pnt1.MelodyPart.duration);
            Debug.Assert(note.note == diatonicTone);
            foreach (var scale in allScales1)
                Debug.Assert(note.note == scale.ChromaticToDiatonic(newNote1));
            foreach (var scale in allScales2)
                Debug.Assert(note.note == scale.ChromaticToDiatonic(newNote2));
            diatonicList.Add(note);
            Debug.Assert(note.note >= -128 && note.note <= 128);

            while (pnt1.Next() && pnt2.Next() && pnt1 != pnt2Start)
            {
                if (pnt1.MelodyPart is NoteWithDuration cnote1 && pnt2.MelodyPart is NoteWithDuration cnote2)
                {
                    if (cnote1.Duration != cnote2.Duration)
                        break;

                    if (cnote1.IsPause != cnote2.IsPause)
                        break;

                    if (!cnote1.IsPause)
                    {
                        int cnote1rel = cnote1.note - pnt1Start.firstNote.note;
                        int cnote2rel = cnote2.note - pnt2Start.firstNote.note;

                        var allScales1New = allScales1.Where(tts => tts.IsInScale(cnote1rel)).ToHashSet();
                        Debug.Assert(allScales1 != null && allScales1.Count() > 0);
                        var allScales2New = allScales2.Where(tts => tts.IsInScale(cnote2rel)).ToHashSet();
                        Debug.Assert(allScales2 != null && allScales2.Count() > 0);
                        if (!allScales1New.Any() || !allScales2New.Any())
                            break;

                        allScales1 = allScales1New;
                        allScales2 = allScales2New;

                        var hs1 = allScales1.Select(tts => tts.ChromaticToDiatonic(cnote1rel)).ToHashSet();
                        var hs2 = allScales2.Select(tts => tts.ChromaticToDiatonic(cnote2rel)).ToHashSet();
                        var hs = hs1.Intersect(hs2).ToHashSet();

                        // It is impossible to have more than one note candidate
                        Debug.Assert(hs.Count <= 1);

                        if (hs.Count == 0)
                            break;

                        int pitch = hs.First();
                        Debug.Assert(-128 <= pitch && pitch <= 128);

                        diatonicList.Add(new NoteWithDuration(pitch, cnote1.Duration));
                    }
                    else diatonicList.Add(new NoteWithDuration(cnote1.Duration));
                }
                else break;
            }

            Debug.Assert(diatonicList.GetNotes().First().note == 0);

            Swap swap = new Swap();
            swap.list = diatonicList;
            swap.pnt1 = pnt1Start;
            swap.pnt2 = pnt2Start;
            swap.scales1 = allScales1;
            swap.scales2 = allScales2;
            return swap;
        }

        /// <summary>
        /// Calculates the subset
        /// </summary>
        /// <param name="noteList"></param>
        /// <param name="allScales"></param>
        /// <returns>The subset of <paramref name="allScales"/> which contains all notes of <paramref name="noteList"/></returns>
        private static HashSet<TwelveToneSet> AllScales(IEnumerable<int> noteList, int newNote, List<TwelveToneSet> allScales)
        {
            // Create TwelveToneSet out of noteList and newNote
            TwelveToneSet tts = new TwelveToneSet(noteList);
            tts = TwelveToneSet.BitOn(tts, new tone12(newNote));

            // Return only those scales covering the TwelveToneSet
            return allScales.Where(scale => tts.CoveredBy(scale)).ToHashSet();
        }
    }
}