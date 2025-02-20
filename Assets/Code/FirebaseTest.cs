using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseTest : MonoBehaviour
{
    private DatabaseReference reference;

    private void Start()
    {
        // Firebaseの初期化
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                reference = FirebaseDatabase.GetInstance(app).RootReference;
                Debug.Log("Firebase initialized successfully.");

                // サンプルデータを送信
                SendTestData();
            }
            else
            {
                Debug.LogError($"Firebase initialization failed: {task.Exception}");
            }
        });
    }

    private void SendTestData()
    {
        // データを書き込むノードを指定して値をセット
        reference.Child("test").SetValueAsync("Hello, Firebase!").ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError($"データ送信に失敗: {task.Exception}");

                // 詳細なエラー情報を出力
                foreach (var innerException in task.Exception.InnerExceptions)
                {
                    Debug.LogError($"Inner Exception: {innerException.Message}");
                    if (innerException is FirebaseException firebaseEx)
                    {
                        Debug.LogError($"Firebase Error Code: {firebaseEx.ErrorCode}");
                    }
                }
            }
            else if (task.IsCompleted)
            {
                Debug.Log("データ送信に成功: Hello, Firebase!");
            }
        });
    }
}
