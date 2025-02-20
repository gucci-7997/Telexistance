using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapbox.Unity.Map;
using Mapbox.Utils;


public class Move : MonoBehaviour
{
    private DatabaseReference reference;
    private bool isFetchingData = false; // データ取得中フラグ
    private string targetGeohash = "";
    public GameObject targetObject;
    public GameObject clone;

    private AbstractMap map; // Mapboxの地図オブジェクト
    private GameObject movingObject; // 移動させるオブジェクト
    /*int i=0;
    int j=0;
    int k=0;
    */

    private async void Start()
    {
        // Beta_Surfaceのオブジェクトを探す（親オブジェクトの下にある場合）
        Transform betaSurfaceTransform = transform.Find("Beta_Surface");  // 親オブジェクトの下にある子オブジェクトを探す

        // Firebase 初期化
        await InitializeFirebase();

        // Mapboxの地図オブジェクトを取得
        map = FindObjectOfType<AbstractMap>();
        movingObject = GameObject.Find("Character"); // ここで移動させたいオブジェクトを指定

        if (movingObject == null)
        {
            Debug.LogError("MovingObject が見つかりません。");
            return;
        }

        // 10秒ごとにデータを取得するコルーチンを開始
        StartCoroutine(RepeatFetchDataEvery10Seconds());
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

    // 10秒ごとにデータを取得するコルーチン
    private System.Collections.IEnumerator RepeatFetchDataEvery10Seconds()
    {
        while (true)
        {
            // 非同期メソッドを呼び出し、完了を待つ
            Vector2d latLon = GetLatLonFromUnityPosition(targetObject.transform.position);
            yield return StartCoroutine(FetchGeohashData(Math.Round(latLon.x, 5), Math.Round(latLon.y, 5)));
            //Debug.Log("データ取得中");
            // 10秒待機
            yield return new WaitForSeconds(10);
        }
    }

    // 位置情報のデータ取得をコルーチンとして実行
    private System.Collections.IEnumerator FetchGeohashData(double lat, double lon)
    {
        if (isFetchingData)
        {
            Debug.Log("データ取得中です。処理をスキップします。");
            yield break; // 処理をスキップ
        }

        isFetchingData = true;

        // ジオハッシュを生成
        targetGeohash = GeohashUtility.Encode(lat, lon, 6); // 精度7のジオハッシュ
        Debug.Log($"lat:{lat},lon:{lon},Generated Geohash: {targetGeohash}");

        // 非同期タスクを使ってデータを取得
        Task<DataSnapshot> task = null;
        try
        {
            task = reference.Child($"geohashes/{targetGeohash}").GetValueAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"データ取得中にエラーが発生しました: {e.Message}");
            isFetchingData = false;
            yield break; // エラーが発生した場合は処理を中断
        }

        // 非同期タスクが完了するのを待機
        yield return new WaitUntil(() => task.IsCompleted);

        // タスクがエラーで終了していないかチェック
        if (task.IsFaulted)
        {
            Debug.LogError($"データ取得中にエラーが発生しました: {task.Exception}");
            isFetchingData = false;
            yield break; // エラーが発生した場合は処理を中断
        }

        // スナップショットを取得
        var snapshot = task.Result;

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
                //Debug.Log($"j={j++}{uniqueId}");

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
//Debug.Log($"i={i++}");
                var locationDataList = entry.Value;

                // 2件以上のデータがある場合のみ計算
                if (locationDataList.Count == 2)
                {
                    // 2件のデータ間で移動を行う
                    var current = locationDataList[0];
                    var next = locationDataList[1];

                    // 時間差を計算（秒単位）
                    float timeDifference = (next.ts - current.ts) ; // タイムスタンプはミリ秒なので、秒に変換
                    if (timeDifference <= 0)
                    {
                        Debug.LogWarning($"時間差が0または負です: {timeDifference}");
                        continue; // 時間差が0または負の場合はスキップ
                    }

                   double distance = LocationUtils.CalculateDistance(current.lat, current.lon, next.lat, next.lon);
                    double bearing = LocationUtils.CalculateBearing(current.lat, current.lon, next.lat, next.lon);
                    double Speed = LocationUtils.CalculateSpeedFromTimestamps(current.lat, current.lon, next.lat, next.lon, current.ts, next.ts);


                    // どの2件のデータを使って計算したかを表示
                    Debug.Log($"Geohash: {targetGeohash}, ID: {current.Id} & {next.Id}, ");
                    Debug.Log($"Data 1: lat={current.lat}, lon={current.lon}, ts={current.ts}");
                    Debug.Log($"Data 2: lat={next.lat}, lon={next.lon}, ts={next.ts}");
                    Debug.Log($"Distance: {distance}m, Bearing: {bearing}°, Speed: {Speed} m/s");

                    // Mapboxで緯度経度を地図座標に変換
                    Vector3 startPosition = map.GeoToWorldPosition(new Vector2d(current.lat, current.lon));
                    Vector3 endPosition = map.GeoToWorldPosition(new Vector2d(next.lat, next.lon));

                    // オブジェクトの移動処理
                    StartCoroutine(MoveObject(clone, startPosition, endPosition, timeDifference ,(float)bearing, Speed)); // timeDifference秒間で移動
                    //Debug.Log($"k={k}");
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

        isFetchingData = false;
    }
// 位置情報間を移動するコルーチン
private System.Collections.IEnumerator MoveObject(GameObject objectToMove,Vector3 start, Vector3 end, float duration, float bearing,double speed)
{
    GameObject movingObjectInstance = Instantiate(objectToMove, start, Quaternion.Euler(0f, bearing, 0f));

// Initialize メソッドで速度を設定
MoveObject moveObjectScript = movingObjectInstance.GetComponent<MoveObject>();
if (moveObjectScript != null)
{
    moveObjectScript.Initialize((float)speed); // speed は呼び出し元で定義された変数
}
else
{
    Debug.LogWarning("MoveObject スクリプトがアタッチされていません。");
}

    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        // 時間に応じて位置を補間
        movingObjectInstance.transform.position = Vector3.Lerp(start, end, elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    // 最後の位置にセット
    movingObjectInstance.transform.position = end;

    // オブジェクトの削除
    Destroy(movingObjectInstance);
}


    // Unityのワールド座標を緯度経度に変換
    private Vector2d GetLatLonFromUnityPosition(Vector3 unityPosition)
    {
        return map.WorldToGeoPosition(unityPosition);
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
