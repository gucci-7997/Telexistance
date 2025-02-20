using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class FirebaseReceiver : MonoBehaviour
{
    private DatabaseReference reference;
    private Dictionary<string, GameObject> existingClones = new Dictionary<string, GameObject>(); // IDに基づいてオブジェクトを追跡
    private Idokeido idokeidoScript;
    public GameObject object3DPrefab; // 3Dオブジェクトのプレハブ

    private bool isFetchingData = false; // データ取得中フラグ

    private double latitude = 35.985897064208984;
    private double longitude = 139.37200927734375;
    private string targetGeohash = "";
    public AbstractMap map; 

    private async void Start()
    {
        // Firebase初期化を待つ
        await InitializeFirebase();

        // Idokeidoスクリプト取得と非同期イベント登録
        idokeidoScript = FindObjectOfType<Idokeido>();
        if (idokeidoScript != null)
        {
            idokeidoScript.OnLocationUpdatedAsync += HandleLocationUpdatedAsync;
        }
    }

    private void OnDestroy()
    {
        // イベント購読解除
        if (idokeidoScript != null)
        {
            idokeidoScript.OnLocationUpdatedAsync -= HandleLocationUpdatedAsync;
        }
    }

    private async Task InitializeFirebase()
    {
        // Firebaseが初期化されるまで待機
        while (!FirebaseApp.CheckAndFixDependenciesAsync().Result.Equals(Firebase.DependencyStatus.Available))
        {
            await Task.Delay(500);
        }

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Initialized.");
    }

int i=0;
    private async Task HandleLocationUpdatedAsync(double newLatitude, double newLongitude)
    {
        latitude = newLatitude;
        longitude = newLongitude;
        targetGeohash = GeohashUtility.Encode(latitude, longitude, 7);
        Debug.Log($"Updated Location: {latitude}, {longitude}, Geohash: {targetGeohash}");

        if (!isFetchingData)
        {
            isFetchingData = true;
            Debug.Log($"Hand Geohash: {targetGeohash}");
            await FetchDataForGeohash(targetGeohash);  // 非同期でデータを取得
            
            isFetchingData = false;
        }
        Debug.Log($"Hand Finish!{i++}回目");
    }
private async Task FetchDataForGeohash(string geohash)
{
    Debug.Log($"Fetching data for geohash: {geohash}");

    // Firebaseで指定されたジオハッシュに完全一致するデータを取得
    var queryTask = reference.Child($"geohashes/{geohash}").GetValueAsync();

    // 完了するまで待機
    DataSnapshot snapshot = await queryTask;

    if (snapshot.Exists)
    {
        Debug.Log($"Data found for geohash {geohash}");

        // 非同期処理を並列化
        List<Task> displayTasks = new List<Task>();

        foreach (DataSnapshot child in snapshot.Children)
        {
            string jsonData = child.GetRawJsonValue();
            LocationData locationData = JsonUtility.FromJson<LocationData>(jsonData);
            Debug.Log($"ID: {locationData.Id}, Latitude: {locationData.lat}, Longitude: {locationData.lon}");

            // DisplayLocationOnMapを非同期で並列実行
            Task displayTask = DisplayLocationOnMap(locationData.Id, locationData.lat, locationData.lon);
            displayTasks.Add(displayTask);
        }

        // 全てのDisplayLocationOnMapが完了するまで待機
        await Task.WhenAll(displayTasks);
    }
    else
    {
        Debug.Log($"No data found for geohash {geohash}");
    }
}

Vector3 worldPos;
private bool isMap = false; // データ取得中フラグ
   private async Task DisplayLocationOnMap(string id, float latitude, float longitude)
{
    // クローンがすでに存在するか確認
    Debug.Log("Display");
    if (existingClones.ContainsKey(id))
    {
        // 既存のオブジェクトを移動
        Debug.Log("Move");
        MoveClone(id, latitude, longitude);
    }
    else
    {
        // 新しいオブジェクトを生成
        if (object3DPrefab != null)
        {
            Debug.Log($"変換前:{latitude},{longitude}");
            // 緯度・経度を Mapbox Unity SDK の座標系に変換
            Vector2d latLon = new Vector2d(latitude, longitude);
            Debug.Log(latLon);

            map.MapVisualizer.OnMapVisualizerStateChanged += (state) =>
        {
            isMap=true;
        };
        Debug.Log("MapVisualize");
            if (!isMap) {
    worldPos = map.GeoToWorldPosition(latLon);
    Debug.Log("Converted world position: " + worldPos);
} else {
    Debug.LogError("Map is not initialized.");
}
isMap=false;
            // インスタンスを作成
            GameObject clone = Instantiate(object3DPrefab, worldPos, Quaternion.identity);
            Debug.Log("Clone done");
            
            // 生成したオブジェクトを辞書に追加
            existingClones[id] = clone;
            Debug.Log($"Created new clone for ID: {id} at position {worldPos}");
        }
        else
        {
            Debug.LogError("Prefab is not assigned!");
        }
    }

    // 位置更新後にフラグを戻す
    isFetchingData = false;
}

private void MoveClone(string id, float latitude, float longitude)
{
    // すでに存在するオブジェクトを移動
    if (existingClones.ContainsKey(id))
    {
        GameObject clone = existingClones[id];
        
        // 新しい位置を計算
        Vector3 newPosition = new Vector3((float)longitude, 0f, (float)latitude);
        
        // オブジェクトを移動
        clone.transform.position = newPosition;
        Debug.Log($"Moved clone {id} to {newPosition}");
    }

    // 移動後にフラグを戻す
    isFetchingData = false;
}


    private void RemoveClone(string id)
    {
        if (existingClones.ContainsKey(id))
        {
            Destroy(existingClones[id]);
            existingClones.Remove(id);
            Debug.Log($"Removed clone: {id}");
        }
    }

    [System.Serializable]
    public class LocationData
    {
        public string Id;
        public float lat;
        public float lon;
        public long ts;
    }
}
