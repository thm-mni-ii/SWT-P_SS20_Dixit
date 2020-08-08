/* created by: SWT-P_SS_20_Dixit */
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;


/// <summary>
/// Represents a QuestionSet in the Game.
/// Communicates with database to retrieve a QuestionSet and its Questions.
/// </summary>
[FirestoreData]
public class QuestionSet
{
    /// <summary>
    /// All Questions in this QuestionSet, stored as references.
    /// Use GetQuestion(int index) to get a Question object
    /// </summary>
    [FirestoreProperty]
    public List<DocumentReference> Questions { get; set; }

    /// <summary>
    /// The docent who created this QuestionSet
    /// </summary>
    [FirestoreProperty]
    public string Docent { get; set; }

    /// <summary>
    /// The module this QuestionSet relats to
    /// </summary>
    [FirestoreProperty]
    public string Module { get; set; }

    /// <summary>
    /// The name of this QuestionSet
    /// </summary>
    [FirestoreProperty]
    public string Name { get; set; }

    /// <summary>
    /// The number of questions in this QuestionSet
    /// </summary>
    public int QuestionCount => Questions.Count;

    /// <summary>
    /// Retrieves question data from DocumentReference stored in Questions
    /// Returns data as Question Object
    /// </summary>
    public Task<Question> GetQuestion(int index)
    {
        return Question.RetrieveQuestion(Questions[index]).ContinueWith(task => task.Result);
    }

    /// <summary>
    /// Retrieves QuestionSet with the specified ID from the database
    /// Returns data as QuestionSet Object
    /// </summary>
    public static Task<QuestionSet> RetrieveQuestionSet(string questionSetID, FirebaseFirestore db)
    {
        DocumentReference docRef = db.Collection("QuestionSets").Document(questionSetID);

        return docRef.GetSnapshotAsync().ContinueWith((task) =>
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
}
