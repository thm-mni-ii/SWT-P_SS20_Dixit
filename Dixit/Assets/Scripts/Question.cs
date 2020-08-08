/* created by: SWT-P_SS_20_Dixit */
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Firestore;


/// <summary>
/// Represents a Question in the Game.
/// Communicates with database to retrieve questions.
/// </summary>
[FirestoreData]
public class Question
{
    /// <summary>
    /// The Question as string
    /// </summary>
    [FirestoreProperty]
    public string QuestionText { get; set; }

    /// <summary>
    /// The level of difficulty of this question
    /// </summary>
    [FirestoreProperty]
    public int Difficulty { get; set; }

    /// <summary>
    /// The correct answer of this question
    /// </summary>
    [FirestoreProperty]
    public string Answer { get; set; }

    /// <summary>
    /// Retrieves question data from the database from DocumentReference
    /// Returns data as Question Object
    /// </summary>
    public static Task<Question> RetrieveQuestion(DocumentReference reference)
    {
        return reference.GetSnapshotAsync().ContinueWith((task) =>
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
