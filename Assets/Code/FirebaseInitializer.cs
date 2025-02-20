using Firebase;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;

public static class FirebaseInitializer
{
    private static bool isFirebaseInitialized = false;

    // Firebaseの初期化を非同期で行う
    public static async Task InitializeFirebaseAsync()
    {
        if (isFirebaseInitialized) return;

        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        await task;

        if (task.Result == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            Debug.Log("初期化:Firebase initialized successfully.");
            isFirebaseInitialized = true;
        }
        else
        {
            Debug.LogError("初期化:Firebase initialization failed.");
        }
    }

    // Firebaseが初期化されているかを確認する
    public static bool IsFirebaseInitialized()
    {
        return isFirebaseInitialized;
    }
}
