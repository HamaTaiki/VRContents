using UnityEngine;

public class HandCheck : MonoBehaviour
{
    private bool action1Performed = false;
    public static bool isAction2Executed = false;
    private float action1Timestamp = 0f;

    void Update()
    {
        float currentRotationY = transform.rotation.eulerAngles.y;
        Debug.Log(currentRotationY);

        // 1. アクション1が行われた場合の条件をチェックする
        if (currentRotationY > 3f && currentRotationY < 20f)
        {
            // アクション1が行われた場合の処理を実行
            Debug.Log("アクション1を起こす");
            action1Performed = true;
            action1Timestamp = Time.time;
        }

        // 2. アクション1が行われてから2秒以内で、かつ条件が満たされている場合、アクション2に推移する
        if (action1Performed && (Time.time - action1Timestamp) <= 1f && currentRotationY < 357f && currentRotationY > 300f)
        {
            // アクション2への推移処理をここに記述
            TransitionToAction2();
        }
    }

    void TransitionToAction2()
    {
        Debug.Log("アクション２を実行");

        isAction2Executed = true;
        action1Performed = false;
        action1Timestamp = 0f;

        Invoke("ResetAction2Flag", 0.5f);
    }

    void ResetAction2Flag()
    {
        isAction2Executed = false;
    }
}
