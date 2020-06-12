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

[FirestoreData]
public class QuestionSet
{
  [FirestoreProperty]
  public List<DocumentReference> Questions { get; set; }
[FirestoreProperty]
  public string Docent { get; set; }
  [FirestoreProperty]
  public string Module { get; set; }
  [FirestoreProperty]
  public string Name { get; set; }
  public int QuestionCount => Questions.Count;

  public static async Task<QuestionSet> RetrieveQuestionSet(string questionSetID, FirebaseFirestore db){
    DocumentReference docRef = db.Collection("questionSets").Document(questionSetID);
    QuestionSet questionSet=null;

    return await docRef.GetSnapshotAsync().ContinueWith<QuestionSet>((task) =>
    {
      if(task.IsFaulted)
      Debug.Log(task.Exception);

      var snapshot = task.Result;
      if (snapshot.Exists)
      {
        Debug.Log(string.Format("Document data for QuestionSet document {0} :", snapshot.Id));

        questionSet = snapshot.ConvertTo<QuestionSet>();
        
      }
      else
      {
        Debug.Log(string.Format("QuestionSet document {0} does not exist!", snapshot.Id));
      }
      return questionSet;
    });
  }

  public async Task<Question> GetQuestion(int index){
    return await Question.RetrieveQuestion(Questions[index]).ContinueWith(task => task.Result);
  }

}
