/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

/// <summary>
/// Utilities used throughout the program
/// </summary>
/// \author SWT-P_SS_20_Dixit
public static class Utils
{
    /// <summary>
    /// Shuffles an array with the Kuth shuffle algorithm
    /// </summary>
    /// <param name="array"> The array to be shuffled</param>
    /// \author SWT-P_SS_20_Dixit
    public static void ShuffleArray<T>(T[] array)
    {
        for (int t = 0; t < array.Length; t++)
        {
            T tmp = array[t];
            int r = Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
    }

    /// <summary>
    /// Generates an array of <c>maxIdx</c> length.
    /// </summary>
    /// <param name="maxIdx"> The length of the array</param>
    /// <returns>The random-number array</returns>
    /// \author SWT-P_SS_20_Dixit
    public static int[] GetRandomQuestionIdxArray(int maxIdx, int numberOfRounds)
    {
        var randomQuestionIdxList = new List<int>(numberOfRounds);

        for (int i = 0; i < numberOfRounds; i++)
        {
            //get a random value which is not in the array yet and place it in the array for the round
            int randomQuestionIdx;
            do
            {
                randomQuestionIdx = Random.Range(0, maxIdx);
            } while (randomQuestionIdxList.Contains(randomQuestionIdx));

            randomQuestionIdxList.Add(randomQuestionIdx);
        }

        return randomQuestionIdxList.ToArray();
    }

    /// <summary>
    /// Gets the list of Players in the current Game.
    /// <returns>The list of players.</returns>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static IEnumerable<Player> GetPlayers() =>
        NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>());

    /// <summary>
    /// Gets the list of Players in the current Game together with their indices.
    /// <returns>The list of players as IEnumerable of ValueTuple<Player, int>.</returns>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static IEnumerable<ValueTuple<Player, int>> GetPlayersIndexed() =>
        GetPlayers().Select(ValueTuple.Create<Player, int>);

    /// <summary>
    /// Gets the NetworkIdentity component of an object with the specified netId
    /// </summary>
    /// <param name="netId"> The Identity to be searched for </param>
    /// <returns> The NetworkIdentity </returns>
    /// \author SWT-P_SS_20_Dixit
    public static NetworkIdentity GetIdentity(UInt32 netId) =>
        NetworkServer.connections.Values.Where(c => c.identity.netId == netId).Select(c => c.identity).First();

    /// <summary>
    /// Checks wether a player has clicked on there own answer
    /// </summary>
    /// <param name="clicker"> The player who clicked on the answer card </param>
    /// <param name="clickedOn"> The answer card the player clicked on </param>
    /// <param name="sameAnswers"> A dictionary to store all duplicate answers </param>
    /// <returns> If the player gave a duplicate answer </returns>
    /// \author SWT-P_SS_20_Dixit
    public static bool ClickedOnOwnAnswer(UInt32 clicker, UInt32 clickedOn, MultivalDictionary<UInt32, UInt32> sameAnswers) =>
        (clicker == clickedOn) || (sameAnswers.ContainsKey(clickedOn) && sameAnswers[clickedOn].Contains(clicker));

    /// <summary>
    /// Checks wether a player gave an answer
    /// </summary>
    /// <param name="p"> The player in question </param>
    /// <param name="answers"> A dictionary that stores all answers </param>
    /// <param name="sameAnswers"> A dictionary to store all duplicate answers </param>
    /// <returns> If the player gave an answer </returns>
    /// \author SWT-P_SS_20_Dixit
    public static bool GaveAnswer(Player p, Dictionary<UInt32, string> answers, MultivalDictionary<UInt32, UInt32> sameAnswers)  =>
        (answers.ContainsKey(p.netId) || sameAnswers.Any(pair => pair.Value.Contains(p.netId)));

    /// <summary>
    /// Checks wether a player gave an empty answer
    /// </summary>
    /// <param name="p"> The player in question </param>
    /// <param name="answers"> A dictionary that stores all answers </param>
    /// <returns> If the player gave an empty answer </returns>
    /// \author SWT-P_SS_20_Dixit
    public static bool AnswerIsEmpty(UInt32 p, Dictionary<UInt32, string> answers) =>
        (answers.ContainsKey(p) && answers[p] == "");

   
    /// <summary>
    /// Checks if anwers are equals.
    /// Ignores Upper and Lower case of the answers.
    /// Consideres numbers lower then 1000 in digits and german words equal.
    /// Uses the levenshtein algorithm for ignoring of 1 spelling mistake.
    /// </summary>
    /// <param name="s1"> The first answer</param>
    /// <param name="s2"> The second answer</param>
    /// <returns>Whehter the answers are equal</returns>
    /// \author SWT-P_SS_20_Dixit
    public static bool AnswersAreEqual(string s1, string s2)
    {
        // numbers as words or digits are equal
        s1 = AllNumbersInGermanWords(s1);
        s2 = AllNumbersInGermanWords(s2);

        if(levenshtein(s1,s2) <= 1)
            return true;

        // do not check lower or upper
        s1 = s1.ToLower();
        s2 = s2.ToLower();

        return s1 == s2;
    }

    private static string AllNumbersInGermanWords(string s)
    {
       
        string[] words = s.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            var num = 0;
            if(int.TryParse(words[i], out num))
            {
                words[i] = numberToGermanWord(num);
            }
        }
    
        return string.Join(" ", words);
    }

    /// <summary>
    /// Converts a number to the german word.
    /// </summary>
    /// <param name="s1"> The number to covert to a word</param>
    /// <returns>The german word for a number.</returns>
    /// \author SWT-P_SS_20_Dixit
    public static string numberToGermanWord(int num) => numberToGermanWord(num, true, false);
    

    private static string numberToGermanWord(int num, bool first, bool isBehindHundert)
    {
        var digitToWord = new string[]{"", "ein", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun"};
        var special = new string[] {"null", "eins", "zwan", "drei", "vier", "fünf", "sech", "sieb", "acht", "neun"};
        
        var s="";

        if (num < 10)
            s = isBehindHundert && num == 1? special[num] : (first && num < 2? special[num] : digitToWord[num]);
        else if (num == 11)
            s = "elf";
        else if (num == 12)
            s = "zwölf";
        else if(num < 20)
            s = (num == 10? "" : special[num%10]) + "zehn";
        else if(num < 100)
        {
            if (num % 10 != 0)
                s = numberToGermanWord(num%10, false, false) + "und";
                       
            s+= special[num/10] + (num/10==3? "ß" : "z") + "ig";
        }
        else if(num < 1000)
        {
            s= digitToWord[num/100] + "hundert" + numberToGermanWord(num%100, false, true);

        }
        else s= num + "";
       
        return s;
    }


    /// <summary>
    /// The Levenshtein Algorithm.
    /// Source: https://www.eximiaco.tech/en/2019/11/17/computing-the-levenshtein-edit-distance-of-two-strings-using-c/.
    /// It take two stings and returns and int for the edit distance.
    /// </summary>
    /// <param name="s1"> The first string</param>
    /// <param name="s2"> The second string</param>
    /// <returns>The edit distance</returns>
    public static int levenshtein(string first,string second)
    {
        if (first.Length == 0)
        {
            return second.Length;
        }

        if (second.Length == 0)
        {
            return first.Length;
        }

        var current = 1;
        var previous = 0;
        var r = new int[2, second.Length + 1];
        for (var i = 0; i <= second.Length; i++)
        {
            r[previous, i] = i;
        }

        for (var i = 0; i < first.Length; i++)
        {
            r[current, 0] = i + 1;

            for (var j = 1; j <= second.Length; j++) 
            { 
                var cost = (second[j - 1] == first[i]) ? 0 : 1; 
                r[current, j] = Min( 
                    r[previous, j] + 1, 
                    r[current, j - 1] + 1, 
                    r[previous, j - 1] + cost ); 
            } 
            previous = (previous + 1) % 2; 
            current = (current + 1) % 2; 
        } 
        return r[previous, second.Length]; 
    } 

    private static int Min(int e1, int e2, int e3) =>
        Math.Min(Math.Min(e1, e2), e3);
}

/// <summary>
/// A Dictionary that contains Lists of type <c>TValue</c> as Values
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class MultivalDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
{
    /// <summary>
    /// Add add a Value to the Dictionary. If the Key allready exists, add the value to the list of values associated with the key
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void Add(TKey key, TValue value)
    {
        if (!this.ContainsKey(key))
            this.Add(key, new List<TValue>());

        this[key].Add(value);
    }
}
