/* created by: SWT-P_SS_20_Dixit */

using NUnit.Framework;

namespace Tests
{
    public class UtilsTest
    {
        [Test]
        public void RandomQuestionTest()
        {
            for (int m = 0; m < 100; m++)
            {
                var randomQuestions = Utils.GetRandomQuestionIdxArray(30, 3);

                for (int i = 0; i < randomQuestions.Length; i++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        Assert.AreNotEqual(randomQuestions[k], randomQuestions[i]);
                    }
                }
            }
        }

        [Test]
        public void NumberToWordTest()
        {
            Assert.AreEqual(Utils.NumberToGermanWord(0), "null");
            Assert.AreEqual(Utils.NumberToGermanWord(1), "eins");
            Assert.AreEqual(Utils.NumberToGermanWord(9), "neun");
            Assert.AreEqual(Utils.NumberToGermanWord(10), "zehn");
            Assert.AreEqual(Utils.NumberToGermanWord(11), "elf");
            Assert.AreEqual(Utils.NumberToGermanWord(12), "zwölf");
            Assert.AreEqual(Utils.NumberToGermanWord(13), "dreizehn");
            Assert.AreEqual(Utils.NumberToGermanWord(16), "sechzehn");
            Assert.AreEqual(Utils.NumberToGermanWord(19), "neunzehn");
            Assert.AreEqual(Utils.NumberToGermanWord(20), "zwanzig");
            Assert.AreEqual(Utils.NumberToGermanWord(21), "einundzwanzig");
            Assert.AreEqual(Utils.NumberToGermanWord(32), "zweiunddreißig");
            Assert.AreEqual(Utils.NumberToGermanWord(40), "vierzig");
            Assert.AreEqual(Utils.NumberToGermanWord(67), "siebenundsechzig");
            Assert.AreEqual(Utils.NumberToGermanWord(75), "fünfundsiebzig");
            Assert.AreEqual(Utils.NumberToGermanWord(99), "neunundneunzig");
            Assert.AreEqual(Utils.NumberToGermanWord(100), "einhundert");
            Assert.AreEqual(Utils.NumberToGermanWord(101), "einhunderteins");
            Assert.AreEqual(Utils.NumberToGermanWord(234), "zweihundertvierunddreißig");
            Assert.AreEqual(Utils.NumberToGermanWord(999), "neunhundertneunundneunzig");
        }

        [Test]
        public void LevenshteinTest()
        {
            Assert.AreEqual(Utils.Levenshtein("fall", "fall"), 0);
            Assert.AreEqual(Utils.Levenshtein("Hallo", "Hello"), 1);
            Assert.AreEqual(Utils.Levenshtein("Ein Stein", "Einstein"), 2);
        }

        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(Utils.AnswersAreEqual("aussage", "Aussage"));
            Assert.IsTrue(Utils.AnswersAreEqual("8 Bit", "acht bit"));
            Assert.IsTrue(Utils.AnswersAreEqual("Es sind 3", "Es sind drei"));
            Assert.IsTrue(Utils.AnswersAreEqual("fünfunddreißig grad", "35 grad"));
            Assert.IsTrue(Utils.AnswersAreEqual("Beinhaltet 18 Liter", "beinhaltet achtzehn Liter"));
            Assert.IsTrue(Utils.AnswersAreEqual("Liter", "Lieter"));
            Assert.IsFalse(Utils.AnswersAreEqual("Ja", "Je"));
            Assert.IsFalse(Utils.AnswersAreEqual("Ein Horn", "Einhorn"));
        }

        [Test]
        public void RemoveSpacesTest()
        {
            Assert.AreEqual("Hello World", Utils.RemoveSpaces("Hello  World"), "2 spaces");
            Assert.AreEqual("Hello World", Utils.RemoveSpaces("Hello   World"), "3 spaces");
            Assert.AreEqual("Hello World", Utils.RemoveSpaces(" Hello World"), "1 space at the start");
            Assert.AreEqual("Hello World", Utils.RemoveSpaces("Hello World "), "1 space at the end");
            Assert.AreEqual("Hello World", Utils.RemoveSpaces(" Hello World "), "1 space at start and end");
            Assert.AreEqual("HelloWorld", Utils.RemoveSpaces(" HelloWorld "), "1 space at start and end (no moddle)");
            Assert.AreEqual("HelloWorld", Utils.RemoveSpaces("HelloWorld "), "1 space at the end (no middle)");
            Assert.AreEqual("HelloWorld", Utils.RemoveSpaces("HelloWorld  "), "2 spaces at the end (no middle)");
            Assert.AreEqual("HelloWorld", Utils.RemoveSpaces("  HelloWorld"), "2 spaces at the start (no middle)");
            Assert.AreEqual("HelloWorld", Utils.RemoveSpaces("  HelloWorld  "), "2 spaces at start and end (no middle)");
            Assert.AreEqual("Hello World foo bar", Utils.RemoveSpaces("Hello  World foo bar"), "2 spaces and more words");
        }
    }
}
