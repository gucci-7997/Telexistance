using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // 作成するオブジェクトのPrefab
    public GameObject prefab;

    // 生成するオブジェクトの親（指定しない場合はルートに生成）
    public Transform parent;

    // データのサンプル（ここではテスト用に設定）
    private List<LocationData> locationDataList = new List<LocationData>
    {
        new LocationData { lat = 35.74764f, lon = 139.80620f, Id = "ID1" },
        new LocationData  { lat = 35.74765f, lon = 139.80630f, Id = "ID2" },
        new LocationData  { lat = 35.74766f, lon = 139.80640f, Id = "ID3" }
    };

    void Start()
    {
        // データに基づいてオブジェクトを生成
        SpawnObjects(locationDataList);
    }

    private void SpawnObjects(List<LocationData> dataList)
    {
        foreach (var data in dataList)
        {
            // 新しいオブジェクトを生成
            GameObject newObject = Instantiate(prefab, parent);

            // 位置を設定（緯度と経度を使って適当な値に変換）
            float x = data.lon ; // スケール調整用
            float z = data.lat ; // スケール調整用
            newObject.transform.position = new Vector3(x, 0, z);

            // オブジェクトの名前をデータIDに変更
            newObject.name = $"Object_{data.Id}";

            // 必要に応じてカスタム処理を追加
            Debug.Log($"Created object for ID: {data.Id}, Position: ({x}, 0, {z})");
        }
    }

    // データ構造の定義
    [System.Serializable]
    public class LocationData
    {
        public float lat;
        public float lon;
        public string Id;
    }
}
