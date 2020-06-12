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

[FirestoreData]
public class Question
{
    [FirestoreProperty]
    public string question { get; set; }

    [FirestoreProperty]
    public string answer { get; set; }

    public static async Task<Question> RetrieveQuestion(DocumentReference reference)
    {
        Question question=null;
        return await reference.GetSnapshotAsync().ContinueWith<Question>((task) =>
        {
            if (task.IsFaulted)
                Debug.Log(task.Exception);

            var snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log(string.Format("Document data for Question document {0} :", snapshot.Id));

                question = snapshot.ConvertTo<Question>();

                Debug.Log(question.question);
                Debug.Log(question.answer);
            }
            else
            {
                Debug.Log(string.Format("Question document {0} does not exist!", snapshot.Id));
            }
            return question;
        });
    }

}
