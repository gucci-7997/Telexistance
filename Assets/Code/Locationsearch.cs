using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI入力用
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class Locationsearch : MonoBehaviour
{
    public AbstractMap map = null;  // Mapboxのマップオブジェクト
    public GameObject ex = null;    // 位置を変更するオブジェクト
    public InputField latitudeInput; // 緯度を入力するUI
    public InputField longitudeInput; // 経度を入力するUI
    public Button updateButton;     // 更新ボタン

    private Vector2d currentLatLng;  // 現在の緯度経度を保存するための変数

    void Start()
    {

        // 更新ボタンにクリック時のイベントを登録
        updateButton.onClick.AddListener(OnUpdateButtonClicked);

/*
        // MapVisualizerが状態変化した際の処理を登録
        map.MapVisualizer.OnMapVisualizerStateChanged += (state) =>
        {
            if (state == ModuleState.Finished)
            {
                // 初期位置を設定
                currentLatLng = new Vector2d(35.985, 139.3725); // 初期値
                UpdateObjectPosition();
            }
        };
        */
    }


    // ボタンがクリックされたときに呼び出される処理
    public void OnUpdateButtonClicked()
    {
        // 入力された緯度経度を取得し解析
        if (double.TryParse(latitudeInput.text, out double lati) &&
            double.TryParse(longitudeInput.text, out double longi))
        {
            currentLatLng = new Vector2d(lati, longi); // 新しい位置を設定
            UpdateObjectPosition(); // オブジェクトを移動
            Debug.Log($"Button clicked. New coordinates: {lati}, {longi}");
        }
        else
        {
            Debug.LogError("Invalid latitude or longitude input.");
        }
    }

    // オブジェクトの位置を更新する関数
    void UpdateObjectPosition()
    {
        // 緯度経度をワールド座標に変換して位置を設定
        Vector3 pos = map.GeoToWorldPosition(currentLatLng);

        if (ex != null)
        {
            ex.transform.position = pos; // オブジェクトの位置を更新
            Debug.Log($"Object moved to new position: {currentLatLng}");
        }
        else
        {
            Debug.LogError("Object to move is not assigned!");
        }
    }
}
