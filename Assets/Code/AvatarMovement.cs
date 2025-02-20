using System.Collections;
using UnityEngine;

public class AvatarMovement : MonoBehaviour
{
    // プレハブを参照
    public GameObject avatarPrefab;  // アバターのプレハブ
    public Transform spawnPoint;     // アバター生成位置（例えばシーンの原点）

    // 動作を制御するspeed変数
    public float speed = 0.5f;

    // アバターが前に動く速度
    public float moveSpeed = 3.0f;

    // Startメソッド
    void Start()
    {
        if (avatarPrefab == null)
        {
            Debug.LogError("avatarPrefabが設定されていません。プレハブをインスペクタで設定してください。");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPointが設定されていません。生成位置をインスペクタで設定してください。");
            return;
        }

        // アバターを3体生成
        for (int i = 0; i < 3; i++)
        {
            GameObject avatar = Instantiate(avatarPrefab, spawnPoint.position + new Vector3(i * 2, 0, 0), Quaternion.identity);
            AvatarController controller = avatar.GetComponent<AvatarController>();
            
            if (controller != null)
            {
                controller.SetAnimationBasedOnSpeed(speed);
            }
            else
            {
                Debug.LogError("アバターにAvatarControllerがアタッチされていません。");
            }
        }

        // アバターを前に動かす
        StartCoroutine(MoveAvatars());
    }

    // アバターを前に動かすコルーチン
    private IEnumerator MoveAvatars()
    {
        // 生成されたアバターを探して動かす
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");  // もしタグを付けていれば

        while (true)
        {
            foreach (GameObject avatar in avatars)
            {
                avatar.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }
}

public class AvatarController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Animatorがアタッチされていません。");
        }
    }

    // speedに応じてアニメーションを設定
    public void SetAnimationBasedOnSpeed(float speed)
    {
        if (animator == null)
        {
            return;
        }

        if (speed < 0.1f)
        {
            animator.Play("idle");
        }
        else if (0.1f <= speed && speed < 1f)
        {
            animator.Play("walk");
        }
        else
        {
            animator.Play("run");
        }
    }
}
