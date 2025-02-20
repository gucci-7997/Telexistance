using System;
using UnityEngine;


public static class LocationUtils
{
    // 2点間の距離を計算する関数（ハフ・サイン法）
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        lat1 = lat1 * Mathf.PI / 180.0;
        lon1 = lon1 * Mathf.PI / 180.0;
        lat2 = lat2 * Mathf.PI / 180.0;
        lon2 = lon2 * Mathf.PI / 180.0;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;
        double a = Mathf.Sin((float)(dLat / 2)) * Mathf.Sin((float)(dLat / 2)) +
                   Mathf.Cos((float)lat1) * Mathf.Cos((float)lat2) *
                   Mathf.Sin((float)(dLon / 2)) * Mathf.Sin((float)(dLon / 2));
        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        double earthRadius = 40000000/2/Mathf.PI; // 地球の半径（メートル）
        return earthRadius * c; // 距離（メートル単位）
    }

    // 2点間の方位角を計算する関数
    public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        double lat1Rad = lat1 * Mathf.PI / 180.0;
        double lon1Rad = lon1 * Mathf.PI / 180.0;
        double lat2Rad = lat2 * Mathf.PI / 180.0;
        double lon2Rad = lon2 * Mathf.PI / 180.0;

        double dLon = lon2Rad - lon1Rad;

        double x = Mathf.Sin((float)dLon) * Mathf.Cos((float)lat2Rad);
        double y = Mathf.Cos((float)lat1Rad) * Mathf.Sin((float)lat2Rad) -
                   Mathf.Sin((float)lat1Rad) * Mathf.Cos((float)lat2Rad) * Mathf.Cos((float)dLon);
        double initialBearing = Mathf.Atan2((float)x, (float)y);
        //return (initialBearing);
        double initialBearingDegrees = initialBearing * 180.0 / Mathf.PI;
        return (initialBearingDegrees + 360) % 360; // 0°〜360°の範囲に正規化
    }

    // タイムスタンプを使用して速度を計算する関数（m/s 単位）
    public static double CalculateSpeedFromTimestamps(double lat1, double lon1, double lat2, double lon2, long timestamp1, long timestamp2)
    {
        // タイムスタンプの差分を計算（秒単位）
        double timeInSeconds = Math.Abs(timestamp2 - timestamp1) ; 
        if (timeInSeconds <= 0)
        {
            timeInSeconds = Math.Abs(timeInSeconds); // 絶対値に変換
        }

        // 距離を計算
        double distance = CalculateDistance(lat1, lon1, lat2, lon2); // メートル単位
        // 速度を計算
        double speed = distance / timeInSeconds; // m/s
        return speed;
    }

    // タイムスタンプを使用して速度を km/h で返す関数
    public static double CalculateSpeedKmhFromTimestamps(double lat1, double lon1, double lat2, double lon2, long timestamp1, long timestamp2)
    {
        double speedMs = CalculateSpeedFromTimestamps(lat1, lon1, lat2, lon2, timestamp1, timestamp2);
        return speedMs * 3.6; // m/s から km/h に変換
    }
}
