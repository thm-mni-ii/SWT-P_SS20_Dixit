using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

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
