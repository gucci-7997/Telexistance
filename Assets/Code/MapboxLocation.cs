using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class MapboxLocation : MonoBehaviour
{
    public AbstractMap map = null;  // Mapboxのマップオブジェクト
    public GameObject ex = null;  // 位置を変更するオブジェクト

    public double latitude = 35.985;   // 動的に変更可能な緯度
    public double longitude = 139.3725; // 動的に変更可能な経度

    private Vector2d currentLatLng;  // 現在の緯度経度を保存するための変数
    private Vector2d previousLatLng; // 前回の緯度経度を保存して、変更を検出するため
    private Location location;

    void Start()
    {
        location=GameObject.Find("Map").GetComponent<Location>();
        latitude=location.latitude;
        longitude=location.longitude;
         Debug.Log("Start latitude:"+latitude+" longitude:"+longitude);
        
        // MapVisualizerが状態変化した際の処理を登録
        map.MapVisualizer.OnMapVisualizerStateChanged += (state) =>
        {
            if (state == ModuleState.Finished)
            {
                // 初期位置を設定
                currentLatLng = new Vector2d(latitude, longitude);
                UpdateCubePosition();
                // Coroutineを開始
            StartCoroutine(UpdateLocationCoroutine());
            }
        };
       
    }

     private IEnumerator UpdateLocationCoroutine()
    {
        while (true)
        {
            if (location.latitude != 0 && location.longitude != 0)
            {
                latitude = location.latitude;
                longitude = location.longitude;
                //Debug.Log("Spawn latitude:"+latitude+" longitude:"+longitude);
        
            }

            // 緯度経度が変更されたら、cubeの位置を更新
            currentLatLng = new Vector2d(latitude, longitude);
            ///if (!currentLatLng.Equals(previousLatLng)) // 前回の位置と異なっていれば更新
            //{
            UpdateCubePosition();
            //    previousLatLng = currentLatLng; // 更新した位置を保存
            //}
            yield return new WaitForSeconds(1); // 10秒間待機
        }
        
    }

    // cubeの位置を更新する関数
    void UpdateCubePosition()
    {
        // 緯度経度をワールド座標に変換して位置を設定
        Vector3 pos = map.GeoToWorldPosition(currentLatLng);
        //Debug.Log($"Updated World Position: {pos}");

        // cubeがnullでないことを確認して位置を設定
        if (ex != null)
        {
            ex.transform.position = pos;
        }
        else
        {
            Debug.LogError("Cube object is not assigned!");
        }
    }
}
