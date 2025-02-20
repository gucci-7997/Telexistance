using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public Transform targetPoint;    // 目標地点（ターゲット）
    public float speed = 1.0f;       // 移動速度
    public float runMultiplier = 2.0f; // 走るときの速度倍率
    private Animator animator;
    private Renderer betaSurfaceRenderer;  // Beta_SurfaceのRenderer

    void Start()
    {
        // Animator コンポーネントを取得
        animator = GetComponent<Animator>();

        // Beta_Surfaceのオブジェクトを探す（親オブジェクトの下にある場合）
        Transform betaSurfaceTransform = transform.Find("Beta_Surface");  // 親オブジェクトの下にある子オブジェクトを探す

        if (betaSurfaceTransform != null)
        {
            betaSurfaceRenderer = betaSurfaceTransform.GetComponent<Renderer>();
            if (betaSurfaceRenderer == null)
            {
                Debug.LogError("Beta_SurfaceにはRendererがありません！");
            }
        }
        else
        {
            Debug.LogError("Beta_Surfaceオブジェクトが見つかりません！");
        }
    }

    void Update()
    {
        // 入力値を取得（Speedの手入力）
        if (Input.GetKey(KeyCode.UpArrow)) speed += 0.1f;  // 上矢印キーで速度アップ
        if (Input.GetKey(KeyCode.DownArrow)) speed -= 0.1f;  // 下矢印キーで速度ダウン

        // speedが0以下の場合は移動しない
        if (speed <= 0)
        {
            speed = 0;  // 速度を0に設定（負の速度を防ぐ）
            animator.SetTrigger("idle");  // 停止アニメーション
            ChangeColorBasedOnSpeed(speed);  // 色を青に変更
            return;  // 移動処理をスキップ
        }

        // 目標地点への方向を計算
        Vector3 direction = targetPoint.position - transform.position;
        direction.y = 0;  // 垂直方向を無視（2D平面上で移動）

        // 目標地点に向かって移動
        if (direction.magnitude > 0.1f)
        {
            // 移動方向の正規化
            direction.Normalize();

            // 移動処理
            transform.position += direction * speed * Time.deltaTime;

            // 移動速度をAnimatorに渡す
            animator.SetFloat("Speed", direction.magnitude * speed);
        }
        else
        {
            // 目標地点に到達した場合、アニメーションを止める
            animator.SetFloat("Speed", 0);
        }

        // 速度に基づいて色を変更
        ChangeColorBasedOnSpeed(speed);

        // スピードに基づいてアニメーションを切り替える
        //ChangeAnimationBasedOnSpeed(speed);

        // デバッグ用に速度を表示
        Debug.Log("Speed: " + speed);
    }

    void ChangeColorBasedOnSpeed(float currentSpeed)
    {
        if (betaSurfaceRenderer != null)
        {
            // speedが0の場合、青色
            if (currentSpeed <= 0.1)
            {
                betaSurfaceRenderer.material.color = Color.blue;
            }
            // speedが1未満の場合、緑色
            else if (currentSpeed > 0.1 && currentSpeed < 1)
            {
                betaSurfaceRenderer.material.color = Color.green;
            }
            // speedが1以上の場合、赤色
            else if (currentSpeed >= 1)
            {
                betaSurfaceRenderer.material.color = Color.red;
            }
        }
    }

/*
    void ChangeAnimationBasedOnSpeed(float currentSpeed)
    {
        if (currentSpeed <= 0.1f)
        {
            animator.SetTrigger("idle");  // Idleアニメーション
        }
        else if (currentSpeed > 0.1f && currentSpeed < 1f)
        {
            animator.SetTrigger("walk");  // 歩きアニメーション
        }
        else if (currentSpeed >= 1f)
        {
            animator.SetTrigger("run");   // 走るアニメーション
        }
    }
    */
}
