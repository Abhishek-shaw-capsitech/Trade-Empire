using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    private void Start()
    {
        StartCoroutine(FirebaseInitialize());
    }
    private IEnumerator FirebaseInitialize()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;
        if(dependencyStatus == DependencyStatus.Available)
        {
            var app = FirebaseApp.DefaultInstance;
            FirebaseDatabase db = FirebaseDatabase.GetInstance("https://trade-empire-game-default-rtdb.firebaseio.com/");


            Debug.Log("Firebase Inialised Successfully");
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: "+dependencyStatus); 
        }
    }
    
}
