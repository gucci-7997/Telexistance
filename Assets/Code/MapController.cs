using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public AbstractMap map = null; 
    // 地図の中心位置を設定するための緯度と経度
    [SerializeField]
    private float latitude = 35.9851f; 
    [SerializeField]
    private float longitude = 139.372469f; 

    private Location location;

    void Start()
    {
        // 緯度経度を指定して地図の中心を設定
        //this.location =FindObjectOfType<Location>();
        //SetMapCenter(location.latitude, location.longitude);
        SetMapCenter(latitude, longitude);
        
    }

    public void SetMapCenter(float lat, float lon)
    {
        // 緯度経度の値を使用して地図の中心を設定します
        map.SetCenterLatitudeLongitude(new Vector2d(lat, lon));
        map.UpdateMap();
    }

    public float sentLati{
        get{return latitude;}
        set { latitude = value; }
    }
    public float sentLong{
        get{return longitude;}
        set { longitude = value; }
    }
}
