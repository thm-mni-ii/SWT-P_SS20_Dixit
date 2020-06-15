/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Collections;
using System.Collections.Generic;
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
    private Lazy<FirebaseFirestore> _db = new Lazy<FirebaseFirestore>(() =>
    {
        // Set up the Editor before calling into the database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://swt-p-ss20-profcollector.firebaseio.com/");

        // Get the root reference location of the database.
        return FirebaseFirestore.DefaultInstance;
    });
    public FirebaseFirestore db => _db.Value;

}
