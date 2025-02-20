using System;
using UnityEngine;

public class GeohashUtility
{
    public static string Encode(double latitude, double longitude, int precision)
    {
        const string base32 = "0123456789bcdefghjkmnpqrstuvwxyz";

        double latMin = -90.0;
        double latMax = 90.0;
        double lonMin = -180.0;
        double lonMax = 180.0;

        string geohash = "";
        int bit = 0;
        int ch = 0;

        while (geohash.Length < precision)
        {
            if (bit % 2 == 1) // 奇数番目: 経度をエンコード
            {
                double mid = (lonMin + lonMax) / 2;
                if (longitude > mid)
                {
                    ch |= (1 << (4 - (bit % 5)));
                    lonMin = mid;
                }
                else
                {
                    lonMax = mid;
                }
            }
            else // 偶数番目: 緯度をエンコード
            {
                double mid = (latMin + latMax) / 2;
                if (latitude > mid)
                {
                    ch |= (1 << (4 - (bit % 5)));
                    latMin = mid;
                }
                else
                {
                    latMax = mid;
                }
            }

            bit++;

            if (bit % 5 == 0) // 5ビットごとに文字に変換
            {
                geohash += base32[ch];
                ch = 0; // 初期化
            }
        }

        Debug.Log(geohash);
        return geohash; // 修正: geohashを返す
    }
}
