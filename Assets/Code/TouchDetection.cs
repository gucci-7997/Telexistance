using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    public int tou=0;
    void Update()
    {
        // タッチ数を確認
        if (Input.touchCount > 0)
{
    // 最初のタッチを取得
    Touch touch = Input.GetTouch(0);
    
    // タッチ位置をスクリーン座標からワールド座標に変換
    Vector2 touchPosition = touch.position;
    Ray ray = Camera.main.ScreenPointToRay(touchPosition);
    RaycastHit hit;
    
    // レイキャスティングでオブジェクトとの衝突をチェック
    if (Physics.Raycast(ray, out hit))
    {
        // 衝突したオブジェクトがこのスクリプトがアタッチされたオブジェクトの場合の処理
        if (hit.collider.gameObject == gameObject)
        {
            // タッチの状態をチェック
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // タッチ中の処理
                tou = 1;
                //Debug.Log("Object is being touched!");
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // タッチが離されたときの処理
                tou = 0;
                //Debug.Log("Object touch ended!");
            }
        }
    }
}
else
{
    // タッチが存在しないときの処理（オプション）
    tou = 0;
}

    }



}
