using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Threading.Tasks;
using System;

public class FirebaseController : MonoBehaviour
{
    private DatabaseReference reference;
    private bool isSending = false; // 送信中かどうかを示すフラグ

    // 位置情報データを格納するクラス
    [System.Serializable]
    public class LocationData
    {
        public float lat;   // 緯度 (float型)
        public float lon;   // 経度 (float型)
        public long ts;     // タイムスタンプ (long型)
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

                // 位置情報サービスを開始
                StartCoroutine(StartLocationService());

                // 10秒ごとにデータを非同期で送信
                StartCoroutine(SendDataEvery10Seconds());
            }
            else
            {
                Debug.LogError("Firebase initialization failed.");
            }
        });
    }

    private IEnumerator StartLocationService()
    {
        // 位置情報サービスが利用可能かどうかを確認
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("LocationService failed to start");
            yield break;
        }

        Input.location.Start();

        // 最大待機時間
        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        Debug.Log("Location service started.");
    }

    private IEnumerator SendDataEvery10Seconds()
    {
        while (true)
        {
            // 前のデータ送信が完了していない場合、待機する
            if (!isSending)
            {
                // 位置情報の更新
                LocationData locationData = GetLocationData();

                if (locationData != null)
                {
                    // データを非同期で送信
                    yield return SendTestDataAsync(locationData);
                }
            }

            yield return new WaitForSeconds(10f); // 10秒待機
        }
    }

    private LocationData GetLocationData()
    {
        // 位置情報を取得
        if (Input.location.status == LocationServiceStatus.Running)
        {
LocationData locationData = new LocationData
{
    lat = (float)Math.Round(Input.location.lastData.latitude, 5),  // doubleからfloatにキャスト
    lon = (float)Math.Round(Input.location.lastData.longitude, 5), // doubleからfloatにキャスト
    ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // タイムスタンプをlong型に変更
};

            return locationData;
        }
        else
        {
            Debug.LogError("Location service not running or location data is unavailable.");
            return null;
        }
    }

    private async Task SendTestDataAsync(LocationData locationData)
    {
        // 送信中フラグを立てる
        isSending = true;

        // JSONデータに変換
        string jsonData = JsonUtility.ToJson(locationData);
        Debug.Log($"Generated JSON data: {jsonData}");

        // デバイスIDを取得
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log($"Device ID: {deviceId}");

        // Firebaseのノードパスを変更
        string path = $"users/{deviceId}/locations/{locationData.ts}"; // timestampをlong型に変更

        // 非同期でデータを書き込む
        var setDataTask = reference.Child(path).SetRawJsonValueAsync(jsonData);

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
