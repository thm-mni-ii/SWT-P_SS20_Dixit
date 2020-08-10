/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Unity.Editor;

/// <summary>
/// Initializes CloudFirestore Default Instance for later use
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class DatabaseSetup : MonoBehaviour
{
    private readonly Lazy<FirebaseFirestore> _db = new Lazy<FirebaseFirestore>(() =>
    {
        // Set up the Editor before calling the database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://swt-p-ss20-profcollector.firebaseio.com/");

        // Get the root reference location of the database.
        return FirebaseFirestore.DefaultInstance;
    });

    /// <summary>
    /// FirebaseFirestore's DefaultInstance; Connection is initialized at first access.
    /// <returns>FirebaseFirestore.DefaultInstance</returns>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public FirebaseFirestore DB => _db.Value;
}

/// <summary>
/// Some extensions to log exceptions from async tasks
/// </summary>
/// \author SWT-P_SS_20_Dixit
public static class TaskExtension
{
    /// <summary>
    /// Appends a new task, that logs task.Exception if task is faulted.
    /// <returns>The new Task object</returns>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static Task ContinueWithLogException(this Task task) => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
            }
        }
    );

    /// <summary>
    /// Appends a new task, that logs task.Exception if task is faulted.
    /// The new Task's result is the result of the given Task or (if faulted) the default value of the given type T.
    /// <returns>The new Task object with the result of the given one</returns>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static Task<T> ContinueWithLogException<T>(this Task<T> task) => task.ContinueWith(t =>
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
