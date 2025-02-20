using UnityEngine;

public class MoveObject : MonoBehaviour
{
    private float speed;
    private Animator animator;
    private Renderer betaSurfaceRenderer;


    private async void Start()
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

    public void Initialize(float initialSpeed)
    {
        speed = initialSpeed;
    }

    void Update()
    {
                            // 移動速度をAnimatorに渡す
                    animator.SetFloat("Speed", (float)speed);
                     // 速度に基づいて色を変更
                    ChangeColorBasedOnSpeed((float)speed);
    }

    void ChangeColorBasedOnSpeed(float currentSpeed)
    {
        if (betaSurfaceRenderer != null)
        {
            // speedが0の場合、青色
            if (currentSpeed <= 0.5)
            {
                betaSurfaceRenderer.material.color = Color.blue;
            }
            // speedが1未満の場合、緑色
            else if (currentSpeed > 0.5 && currentSpeed < 1.39)
            {
                betaSurfaceRenderer.material.color = Color.green;
            }
            // speedが1以上の場合、赤色
            else if (currentSpeed >= 1.39)
            {
                betaSurfaceRenderer.material.color = Color.red;
            }
        }
    }
}
