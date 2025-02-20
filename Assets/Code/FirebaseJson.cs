using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;

public class FirebaseJson : MonoBehaviour
{
    private DatabaseReference reference;

    // 位置情報データを格納するクラス
    [System.Serializable]
    public class LocationData
    {
        public float latitude;
        public float longitude;
        public double timestamp;
    }

    private async void Start()
    {
        // Firebaseの初期化
        await FirebaseApp.CheckAndFixDependenciesAsync();

        if (FirebaseApp.DefaultInstance != null)
        {
            reference = FirebaseDatabase.GetInstance(FirebaseApp.DefaultInstance).RootReference;
            Debug.Log("Firebase initialized successfully.");

            // サンプルデータを非同期で送信
            await SendTestDataAsync();
        }
        else
        {
            Debug.LogError("Firebase initialization failed.");
        }
    }

    private async Task SendTestDataAsync()
    {
        // 位置情報データを作成
        LocationData locationData = new LocationData
        {
            latitude = 35.6895f,  // 例: 東京の緯度
            longitude = 139.6917f, // 例: 東京の経度
            timestamp = (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalMilliseconds
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
    }
}
