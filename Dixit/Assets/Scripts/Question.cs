/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Unity.Editor;


/// <summary>
/// Represents a Question in the Game.
/// Communicates with database to retrieve questions.
/// </summary>
[FirestoreData]
public class Question
{
    [FirestoreProperty]
    public string QuestionText { get; set; }

    [FirestoreProperty]
    public int Difficulty { get; set; }

    [FirestoreProperty]
    public string Answer { get; set; }

    /// <summary>
    /// Retrieves question data from the database from DocumentReference
    /// Returns data as Question Object 
    /// </summary>
    public static async Task<Question> RetrieveQuestion(DocumentReference reference)
    {
        return await reference.GetSnapshotAsync().ContinueWith<Question>((task) =>
        {
            if (task.IsFaulted) throw task.Exception;

            var snapshot = task.Result;
            if (!snapshot.Exists)
            {
                Debug.Log(string.Format("Question document {0} does not exist!", snapshot.Id));
                return null;
            }
            return snapshot.ConvertTo<Question>();
        });
    }
}
