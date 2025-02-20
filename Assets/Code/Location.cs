using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;


public class Location : MonoBehaviour
{
    public static Location Instance { set; get; }
    private string deviceId;

    public float latitude;
    public float longitude;
    double timestamp;

 private void Start()
{
            DontDestroyOnLoad(gameObject);
            StartCoroutine(StartLocationService());

}


private IEnumerator StartLocationService()
{
   if (Input.location.status == LocationServiceStatus.Failed)
{
    Debug.Log("LocationService failed to start");
    yield break;
}


    Input.location.Start();

    int maxWait = 20;
    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
    {
        yield return new WaitForSeconds(1);
        maxWait--;
    }

    if (maxWait <= 0)
    {
        Debug.Log("Timed out");
        yield break;
    }

    if (Input.location.status == LocationServiceStatus.Failed)
    {
        Debug.Log("Unable to determine device location");
        yield break;
    }

    // ここで位置情報の更新を開始
        while (true) {
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        timestamp = Input.location.lastData.timestamp;
        Debug.Log($"Latitude: {latitude}, Longitude: {longitude},  timestamp: {timestamp}");
       

        }
    }

}