using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class FirebaseCon : MonoBehaviour
{
    private DatabaseReference reference;
    private bool isSending = false; // 送信中フラグ
    private static readonly int LOCATION_SEND_INTERVAL = 10; // 位置情報送信間隔（秒）
    public Text locationText; // UIテキストを表示するための参照を追加

    [System.Serializable]
    public class LocationData
    {
        public double lat;
        public double lon;
        public long ts;
        public string Id;
    }

    private void Start()
    {
        // Firebaseの初期化を一度だけ行う
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        // Firebaseの初期化を非同期で行う
        Task initializeTask = FirebaseInitializer.InitializeFirebaseAsync();
        
        // 初期化が完了するまで待機
        yield return new WaitUntil(() => initializeTask.IsCompleted);

        if (initializeTask.Status == TaskStatus.RanToCompletion)
        {
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase Initialized.");

            // 位置情報サービスの開始
            StartCoroutine(StartLocationService(OnLocationReady, OnLocationFailed));

            // 位置情報が準備できたらデータ送信を開始
            StartCoroutine(SendDataEveryInterval(LOCATION_SEND_INTERVAL));
        }
        else
        {
            Debug.LogError("Firebase initialization failed.");
        }
    }

    // 位置情報サービスを開始するコルーチン
    private IEnumerator StartLocationService(Action onLocationReady, Action onLocationFailed)
    {
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            onLocationFailed?.Invoke();
            yield break;
        }

        Input.location.Start();

        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            onLocationFailed?.Invoke();
            yield break;
        }

        onLocationReady?.Invoke();
    }

    // 位置情報取得成功時の処理
    private void OnLocationReady()
    {
        Debug.Log("Location service started.");
    }

    // 位置情報取得失敗時の処理
    private void OnLocationFailed()
    {
        Debug.LogError("LocationService failed to start.");
    }

    // 指定した間隔で位置情報を送信するコルーチン
    private IEnumerator SendDataEveryInterval(int intervalSeconds)
    {
        while (true)
        {
            if (!isSending)
            {
                LocationData locationData = GetLocationData();

                if (locationData != null)
                {
                    yield return SendTestDataAsync(locationData);
                }
            }

            yield return new WaitForSeconds(intervalSeconds); // intervalSeconds 秒ごとに送信
        }
    }

    // 位置情報を取得する
    private LocationData GetLocationData()
    {
        LocationData locationData = new LocationData();

if (Input.location.status == LocationServiceStatus.Running)
{
    locationData.lat = Math.Round(Input.location.lastData.latitude, 5);  // 小数点第5位までに丸めてfloat型に変換
    locationData.lon = Math.Round(Input.location.lastData.longitude, 5); // 小数点第5位までに丸めてfloat型に変換
    locationData.ts = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds; // UNIXタイムスタンプに変更
    locationData.Id = SystemInfo.deviceUniqueIdentifier; // デバイスIDを取得
}
else
{
    // 位置情報が取得できない場合は、デフォルト値（1）を設定
    locationData.lat = 1;  
    locationData.lon = 1; 
    locationData.ts = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds; // UNIXタイムスタンプに変更
    locationData.Id = SystemInfo.deviceUniqueIdentifier;
    Debug.Log("位置情報が取得できませんでした。デフォルト値（1）を送信します。");
}
UpdateLocationText(locationData);
return locationData;

    }
    // 位置情報をUIテキストに表示
    private void UpdateLocationText(LocationData locationData)
    {
        if (locationText != null)
        {
            locationText.text = $"Latitude: {locationData.lat:F5}\n" +
                                $"Longitude: {locationData.lon:F5}\n" +
                                $"Timestamp: {locationData.ts}\n" +
                                $"ID: {locationData.Id}";
        }
    }

    // Firebaseにデータを送信する
    private async Task SendTestDataAsync(LocationData locationData)
    {
        isSending = true;

        string geohash = GeohashUtility.Encode(locationData.lat, locationData.lon, 6); // Geohashをエンコード
        string jsonData = JsonUtility.ToJson(locationData);

        string key = $"{locationData.Id}_{locationData.ts}";
        string path = $"geohashes/{geohash}/{key}";

        var setDataTask = reference.Child(path).SetRawJsonValueAsync(jsonData);

        await setDataTask;

        if (setDataTask.IsFaulted)
        {
            Debug.LogError($"データ送信に失敗: {setDataTask.Exception}");
        }
        else
        {
            Debug.Log("データ送信に成功!");

            // 送信完了後にデータを受信して表示
            //await FetchDataForGeohash(geohash);
        }

        isSending = false;
    }

    // 指定されたGeohashのデータを取得し表示する
    private async Task FetchDataForGeohash(string geohash)
    {
        Debug.Log($"Fetching data for geohash: {geohash}");

        var queryTask = reference.Child($"geohashes/{geohash}").OrderByKey().GetValueAsync();
        DataSnapshot snapshot = await queryTask;

        if (snapshot.Exists)
        {
            Debug.Log($"Data found for geohash {geohash}");

            foreach (DataSnapshot child in snapshot.Children)
            {
                string jsonData = child.GetRawJsonValue();
                LocationData locationData = JsonUtility.FromJson<LocationData>(jsonData);
                Debug.Log($"ID: {locationData.Id}, Latitude: {locationData.lat}, Longitude: {locationData.lon}");
                // ここで位置情報をマップ上に表示する処理を追加
            }
        }
        else
        {
            Debug.Log($"No data found for geohash {geohash}");
        }
    }
}
