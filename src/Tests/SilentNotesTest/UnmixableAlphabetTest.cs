using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Workers;

namespace SilentNotesTest
{
    [TestClass]
    public class UnmixableAlphabetTest
    {
        [TestMethod]
        public void IsOfCorrectAlphabetWorksCorrectly()
        {
            for (int i = 0; i < 512; i++)
            {
                bool evaluated = UnmixableAlphabet.IsOfCorrectAlphabet((char)i);
                bool expected = IsOfCorrectAlphabet((char)i);
                Assert.AreEqual(expected, evaluated);
            }
        }

        private static bool IsOfCorrectAlphabet(char letter)
        {
            // Independend method without usage of fast binary search.
            int index = Array.IndexOf(UnmixableAlphabet.Characters, letter);
            return index >= 0;
        }

    }
}
