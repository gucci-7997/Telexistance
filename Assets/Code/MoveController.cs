using System.Collections;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class MoveController : MonoBehaviour
{
    private Animator animator;
    private Renderer betaSurfaceRenderer;
    private AbstractMap map;

    private void Awake()
    {
        // Animator コンポーネントを取得
        animator = GetComponent<Animator>();

        // Beta_SurfaceのRenderer取得
        Transform betaSurfaceTransform = transform.Find("Beta_Surface");
        if (betaSurfaceTransform != null)
        {
            betaSurfaceRenderer = betaSurfaceTransform.GetComponent<Renderer>();
        }
        else
        {
            Debug.LogError("Beta_Surfaceオブジェクトが見つかりません！");
        }

        // Mapboxの地図オブジェクトを取得
        map = FindObjectOfType<AbstractMap>();
    }

    public void StartMovement(Vector2d startLatLon, Vector2d endLatLon, float duration, float bearing, float speed)
    {
        // Animator に速度を設定
        animator.SetFloat("Speed", speed);

        // スピードに応じた色を設定
        ChangeColorBasedOnSpeed(speed);

        // Mapboxで緯度経度をUnityの位置に変換
        Vector3 startPosition = map.GeoToWorldPosition(startLatLon);
        Vector3 endPosition = map.GeoToWorldPosition(endLatLon);

        // オブジェクトの移動を開始
        StartCoroutine(MoveObject(startPosition, endPosition, duration, bearing));
    }

    private IEnumerator MoveObject(Vector3 start, Vector3 end, float duration, float bearing)
    {
        float elapsedTime = 0f;

        // 向きを変更
        transform.rotation = Quaternion.Euler(0f, bearing, 0f);

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }

    private void ChangeColorBasedOnSpeed(float speed)
    {
        if (betaSurfaceRenderer != null)
        {
            if (speed <= 0.5f) betaSurfaceRenderer.material.color = Color.blue;
            else if (speed > 0.5f && speed < 1.39f) betaSurfaceRenderer.material.color = Color.green;
            else betaSurfaceRenderer.material.color = Color.red;
        }
    }
}
