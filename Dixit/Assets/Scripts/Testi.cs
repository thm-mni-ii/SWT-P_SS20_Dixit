using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using UnityEngine.UI;

public class Testi : MonoBehaviour
{
    // Start is called before the first frame update
    public Text m_SomeText;
   public void DoSomething() {
        FirebaseDatabase.DefaultInstance
       .GetReference("users")
       .GetValueAsync().ContinueWith(task => {
           if (task.IsFaulted)
           {
              // Handle the error...
          }
           else if (task.IsCompleted)
           {
               DataSnapshot snapshot = task.Result;
               // Do something with snapshot...
               m_SomeText.text = (string)snapshot.Child("/Dren/email").GetValue(false);
          }
       });

    }
}
