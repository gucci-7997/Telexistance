using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GeohashDataFetcher : MonoBehaviour
{
    private DatabaseReference reference;
    private bool isFetchingData = false; // データ取得中フラグ
    private string targetGeohash = "";
    private double latitude = 35.74766; // 初期の緯度
    private double longitude = 139.80625; // 初期の経度

    private async void Start()
    {
        // Firebase 初期化
        await InitializeFirebase();

        // 指定された位置情報を使用してデータを取得
        await FetchGeohashData(latitude, longitude);
    }

    private async Task InitializeFirebase()
    {
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != Firebase.DependencyStatus.Available)
        {
            Debug.LogError($"Firebase の初期化に失敗: {dependencyStatus}");
            return;
        }

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Initialized.");
    }
public async Task FetchGeohashData(double lat, double lon)
{
    if (isFetchingData)
    {
        Debug.Log("データ取得中です。処理をスキップします。");
        return;
    }

    isFetchingData = true;

    // ジオハッシュを生成
    targetGeohash = GeohashUtility.Encode(lat, lon, 7); // 精度7のジオハッシュ
    Debug.Log($"Generated Geohash: {targetGeohash}");

    try
    {
        var snapshot = await reference.Child($"geohashes/{targetGeohash}").GetValueAsync();

        if (snapshot.Exists)
        {
            Debug.Log($"Data found for geohash {targetGeohash}");

            // 固有IDごとのデータを格納する辞書
            Dictionary<string, List<LocationData>> groupedData = new Dictionary<string, List<LocationData>>();

            // geohash_{固有ID}_{timestamp} の下にあるデータを取得
            foreach (DataSnapshot child in snapshot.Children)
            {
                string jsonData = child.GetRawJsonValue();
                LocationData locationData = JsonUtility.FromJson<LocationData>(jsonData);

                // 固有IDを抽出（{固有ID}_{timestamp}形式なので、最初の部分が固有ID）
                string uniqueId = child.Key.Split('_')[0];

                // 固有IDがnullまたは空でないか確認
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogWarning($"無効な固有IDのデータがあります: {jsonData}");
                    continue; // 無効なデータをスキップ
                }

                // 固有IDをキーにしてデータをグループ化
                if (!groupedData.ContainsKey(uniqueId))
                {
                    groupedData[uniqueId] = new List<LocationData>();
                }
                groupedData[uniqueId].Add(locationData);
            }

            // 固有IDごとに計算処理
            foreach (var entry in groupedData)
            {
                var locationDataList = entry.Value;

                // 2件以上のデータがある場合のみ計算
                if (locationDataList.Count == 2)
                {
                    // 2件のデータ間で速度と方位角を計算
                    var current = locationDataList[0];
                    var next = locationDataList[1];

                    double distance = LocationUtils.CalculateDistance(current.lat, current.lon, next.lat, next.lon);
                    double bearing = LocationUtils.CalculateBearing(current.lat, current.lon, next.lat, next.lon);
                    double speedms = LocationUtils.CalculateSpeedFromTimestamps(current.lat, current.lon, next.lat, next.lon, current.ts, next.ts);

                    // どの2件のデータを使って計算したかを表示
                    Debug.Log($"Geohash: {targetGeohash}, ID: {current.Id} & {next.Id}, ");
                    Debug.Log($"Data 1: lat={current.lat}, lon={current.lon}, ts={current.ts}");
                    Debug.Log($"Data 2: lat={next.lat}, lon={next.lon}, ts={next.ts}");
                    Debug.Log($"Distance: {distance}m, Bearing: {bearing}°, Speed: {speedms} m/s");
                }
                else
                {
                    Debug.Log($"固有ID {entry.Key} のデータは2件ではないため、計算は行いません。");
                }
            }
        }
        else
        {
            Debug.Log($"No data found for geohash {targetGeohash}");
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"データ取得中にエラーが発生しました: {e.Message}");
    }
    finally
    {
        isFetchingData = false;
    }
}

[System.Serializable]
public class LocationData
{
    public string Id;  // 固有ID
    public double lat;
    public double lon;
    public long ts;   // タイムスタンプ
}


}