using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MusicCore.Tests.Melody
{
    [TestClass]
    public class MelodyCompositeTest
    {
        [TestMethod]
        public void TestComposite()
        {
            MelodyBase melody = new MelodyAtomic(new object[] { 0, 1, 2, 0 });
            MelodyComposite composite = new MelodyComposite("AA+", melody);

            NoteList noteList = new NoteList(new object[] { 0, 1, 2, 0, 1, 2, 3, 1 });
            EqualNotes(noteList, composite);
        }

        private static void EqualNotes(NoteList expectedNotes, MelodyBase melody)
        {
            IEnumerator<NoteWithDuration> enumerator = melody.Notes().GetEnumerator();
            foreach (Note note in expectedNotes.Notes())
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(note.note, enumerator.Current.note);
                Assert.AreEqual(note.alter, enumerator.Current.alter);
                Assert.AreEqual(note.IsPause, enumerator.Current.IsPause);
            }
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}