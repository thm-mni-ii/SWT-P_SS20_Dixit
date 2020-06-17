/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Unity.Editor;

/// <summary>
/// Initializes CloudFirestore Default Instance for later use 
/// </summary>
public class DatabaseSetup : MonoBehaviour
{
    private readonly Lazy<FirebaseFirestore> _db = new Lazy<FirebaseFirestore>(() =>
    {
        // Set up the Editor before calling into the database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://swt-p-ss20-profcollector.firebaseio.com/");

        // Get the root reference location of the database.
        return FirebaseFirestore.DefaultInstance;
    });

    public FirebaseFirestore DB => _db.Value;
}

public static class TaskExtention
{
    public static Task ContinueWithLogException(this Task task) => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
            }
        }
    );

    public static Task<T> ContinueWithLogException<T>(this Task<T> task) => task.ContinueWith<T>(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
                return default;
            }

            return t.Result;
        }
    );
}
