using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class ObjectLocation : MonoBehaviour
{
    // Mapboxの地図を参照
    public AbstractMap map;

    // 対象オブジェクト
    public GameObject targetObject;

    void Update()
    {
        // 緯度経度を取得
        Vector2d latLon = GetLatLonFromUnityPosition(targetObject.transform.position);
        Debug.Log("緯度: " + latLon.x + ", 経度: " + latLon.y);
    }

    // Unityのワールド座標を緯度経度に変換
    private Vector2d GetLatLonFromUnityPosition(Vector3 unityPosition)
    {
        return map.WorldToGeoPosition(unityPosition);
    }
}
