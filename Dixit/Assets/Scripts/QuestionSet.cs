/* created by: SWT-P_SS_20_Dixit */
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;


/// <summary>
/// Represents a QuestionSet in the Game.
/// Communicates with database to retrieve a QuestionSet and its Questions.
/// </summary>
/// \author SWT-P_SS_20_Dixit
[FirestoreData]
public class QuestionSet
{
    /// <summary>
    /// All Questions in this QuestionSet, stored as references.
    /// </summary>
    /// <remarks>Use GetQuestion(int index) to get a Question object.</remarks>
    /// \author SWT-P_SS_20_Dixit
    [FirestoreProperty]
    public List<DocumentReference> Questions { get; set; }

    /// <summary>
    /// The docent who created this QuestionSet
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [FirestoreProperty]
    public string Docent { get; set; }

    /// <summary>
    /// The module this QuestionSet relats to
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [FirestoreProperty]
    public string Module { get; set; }

    /// <summary>
    /// The name of this QuestionSet
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [FirestoreProperty]
    public string Name { get; set; }

    /// <summary>
    /// The number of questions in this QuestionSet
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int QuestionCount => Questions.Count;

    /// <summary>
    /// Retrieves question data from DocumentReference stored in #Questions at <paramref name="index" />.
    /// </summary>
    /// <returns>A Task retriving the requested Question Object</returns>
    /// \author SWT-P_SS_20_Dixit
    public Task<Question> GetQuestion(int index)
    {
        return Question.RetrieveQuestion(Questions[index]).ContinueWith(task => task.Result);
    }

    /// <summary>
    /// Retrieves QuestionSet with the specified id from the database
    /// Returns data as QuestionSet Object
    /// </summary>
    /// <param name="questionSetID">The id of the question set</param>
    /// <param name="db">The FirebaseFirestore instance, usually <c>FirebaseFirestore.DefaultInstance</c></param>
    /// \author SWT-P_SS_20_Dixit
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
