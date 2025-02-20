using System;
using System.Collections.Generic;
using System.Linq; // 追加
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Collections;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // 生成するPrefab
    private DatabaseReference reference; // Firebase Databaseの参照
    private bool isFetchingData = false; // データ取得中フラグ
    private string targetGeohash = ""; // 取得するジオハッシュ

    private async void Start()
    {
        // Firebase初期化
        await InitializeFirebase();

        if (reference == null)
        {
            Debug.LogError("Firebase Database Reference is null after initialization!");
            return;
        }

        // Prefabを定期的に生成
        StartCoroutine(SpawnPrefabsAtInterval());
    }

    private async System.Threading.Tasks.Task InitializeFirebase()
    {
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != Firebase.DependencyStatus.Available)
        {
            Debug.LogError($"Firebaseの初期化に失敗: {dependencyStatus}");
            return;
        }

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Initialized.");
    }

    private IEnumerator SpawnPrefabsAtInterval()
    {
        while (true)
        {
            // 非同期メソッドを呼び出し、データ取得
            yield return StartCoroutine(FetchGeohashData(35.74766, 139.80625)); // 仮の緯度・経度を使用

            // 10秒間隔でPrefab生成を繰り返す
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator FetchGeohashData(double lat, double lon)
    {
        if (isFetchingData)
        {
            Debug.Log("データ取得中です。処理をスキップします。");
            yield break;
        }

        isFetchingData = true;

        // ジオハッシュを生成
        targetGeohash = GeohashUtility.Encode(lat, lon, 6); // 精度6のジオハッシュ
        if (string.IsNullOrEmpty(targetGeohash))
        {
            Debug.LogError("targetGeohash is null or empty!");
            isFetchingData = false;
            yield break;
        }

        if (reference == null)
        {
            Debug.LogError("Firebase Database Reference is null!");
            isFetchingData = false;
            yield break;
        }

        Debug.Log($"Fetching data for geohash: {targetGeohash}");

        var task = reference.Child($"geohashes/{targetGeohash}").GetValueAsync();
        if (task == null)
        {
            Debug.LogError($"Failed to create Firebase task for geohash {targetGeohash}");
            isFetchingData = false;
            yield break;
        }

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Firebase task encountered an error: {task.Exception}");
            isFetchingData = false;
            yield break;
        }

        var snapshot = task.Result;
        if (snapshot == null || !snapshot.Exists)
        {
            Debug.LogWarning($"No data found for geohash {targetGeohash}");
            isFetchingData = false;
            yield break;
        }

        // 修正: Childrenの数を取得する際に System.Linq の Count() を使用
        if (!snapshot.Children.Any())
        {
            Debug.LogWarning($"No children found for geohash {targetGeohash}");
            isFetchingData = false;
            yield break;
        }

        Debug.Log($"Data found for geohash {targetGeohash}");
        foreach (DataSnapshot child in snapshot.Children)
        {
            string jsonData = child.GetRawJsonValue();
            Debug.Log($"Child key: {child.Key}, data: {jsonData}");

            // Prefabを生成
            SpawnPrefabAtLocation(lat, lon);
        }

        isFetchingData = false;
    }

    private void SpawnPrefabAtLocation(double lat, double lon)
    {
        // 緯度・経度をUnityのワールド座標に変換する処理が必要
        Vector3 position = ConvertLatLonToWorldPosition(lat, lon);

        // Prefabを生成
        GameObject spawnedPrefab = Instantiate(prefabToSpawn, position, Quaternion.identity);
        Debug.Log($"Spawned prefab at lat: {lat}, lon: {lon}");
    }

    private Vector3 ConvertLatLonToWorldPosition(double lat, double lon)
    {
        // 仮の実装。使用する地図システム（Mapboxなど）に応じて適切に変換してください。
        return new Vector3((float)lon, 0, (float)lat);
    }
}
