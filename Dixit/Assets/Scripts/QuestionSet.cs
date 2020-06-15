/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Unity.Editor;


/// <summary>
/// Represents a QuestionSet in the Game.
/// Communicates with database to retrieve a QuestionSet and its Questions.
/// </summary>
[FirestoreData]
public class QuestionSet
{
    //Questions are stored as references and can later be accessed with getQuestion(...)
    [FirestoreProperty]
    public List<DocumentReference> Questions { get; set; }

    [FirestoreProperty]
    public string Docent { get; set; }

    [FirestoreProperty]
    public string Module { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    public int QuestionCount => Questions.Count;

    /// <summary>
    /// Retrieves QuestionSet data from the database from ID
    /// Returns data as QuestionSet Object 
    /// </summary>
    public static async Task<QuestionSet> RetrieveQuestionSet(string questionSetID, FirebaseFirestore db)
    {
        DocumentReference docRef = db.Collection("questionSets").Document(questionSetID);

        return await docRef.GetSnapshotAsync().ContinueWith<QuestionSet>((task) =>
        {
            if (task.IsFaulted) throw task.Exception;

            var snapshot = task.Result;
            if (!snapshot.Exists)
            {
                Debug.Log(string.Format("QuestionSet document {0} does not exist!", snapshot.Id));
                return null;
            }
            return snapshot.ConvertTo<QuestionSet>();
        });
    }

    /// <summary>
    /// Retrieves question data from DocumentReference stored in Questions
    /// Returns data as Question Object 
    /// </summary>
    public async Task<Question> GetQuestion(int index)
    {
        return await Question.RetrieveQuestion(Questions[index]).ContinueWith(task => task.Result);
    }

}
