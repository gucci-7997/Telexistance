using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Threading.Tasks;

public class FirebaseHidouki : MonoBehaviour
{
    private DatabaseReference reference;
    private bool isSending = false; // 送信中かどうかを示すフラグ

    // 位置情報データを格納するクラス
    [System.Serializable]
    public class LocationData
    {
        public float latitude;
        public float longitude;
        public double timestamp;
    }

    private void Start()
    {
        // Firebaseの初期化
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                reference = FirebaseDatabase.GetInstance(app).RootReference;
                Debug.Log("Firebase initialized successfully.");

                // 10秒ごとにデータを非同期で送信
                StartCoroutine(SendDataEvery10Seconds());
            }
            else
            {
                Debug.LogError("Firebase initialization failed.");
            }
        });
    }

    private IEnumerator SendDataEvery10Seconds()
    {
        while (true)
        {
            // 前のデータ送信が完了していない場合、待機する
            if (!isSending)
            {
                // データを非同期で送信
                yield return SendTestDataAsync(); // コルーチン内で非同期メソッドを呼び出す
            }
            yield return new WaitForSeconds(10f); // 10秒待機
        }
    }

    private async Task SendTestDataAsync()
    {
        // 送信中フラグを立てる
        isSending = true;

        // 位置情報データを作成
        LocationData locationData = new LocationData
        {
            latitude = 35.6895f,  // 例: 東京の緯度
            longitude = 139.6917f, // 例: 東京の経度
            timestamp = System.DateTime.UtcNow.Ticks  // timestampをlong型のTicksに変更
        };

        // JSONデータに変換
        string jsonData = JsonUtility.ToJson(locationData);
        Debug.Log($"Generated JSON data: {jsonData}");

        // 非同期でデータを書き込む
        var setDataTask = reference.Child("locations").SetRawJsonValueAsync(jsonData);

        // 送信の完了を待つ
        await setDataTask;

        if (setDataTask.IsFaulted)
        {
            Debug.LogError($"データ送信に失敗: {setDataTask.Exception}");
        }
        else
        {
            Debug.Log("データ送信に成功!");
        }

        // 送信が完了したらフラグをリセット
        isSending = false;
    }
}
