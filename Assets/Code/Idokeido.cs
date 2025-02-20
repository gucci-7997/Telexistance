using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Threading.Tasks;

public class Idokeido : MonoBehaviour
{
    public delegate Task LocationUpdatedAsync(double latitude, double longitude); // 非同期デリゲート
    public event LocationUpdatedAsync OnLocationUpdatedAsync; // 非同期イベント

    private MapController mapController;
    public AbstractMap map;
    public GameObject targetObject;
    private double latitude, longitude;
    private Coroutine positionUpdateCoroutine;

    void Start()
    {
        this.mapController = FindObjectOfType<MapController>();
        positionUpdateCoroutine = StartCoroutine(UpdatePositionEvery10Seconds());
    }

    private IEnumerator UpdatePositionEvery10Seconds()
    {
        // 非同期で位置更新を行う
        while (true)
        {
            Vector3 targetPosition = targetObject.transform.position;
            Vector2d latLon = map.WorldToGeoPosition(targetPosition);

            latitude = latLon.x;
            longitude = latLon.y;

            // 非同期イベントを発火
            if (OnLocationUpdatedAsync != null)
            {
                // 非同期メソッド呼び出し
                Task.Run(async () =>
                {
                    await OnLocationUpdatedAsync?.Invoke(latitude, longitude);
                });
            }

            yield return new WaitForSeconds(10);
        }
    }
}
