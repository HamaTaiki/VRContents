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

        // 1. �A�N�V����1���s��ꂽ�ꍇ�̏������`�F�b�N����
        if (currentRotationY > 3f && currentRotationY < 20f)
        {
            // �A�N�V����1���s��ꂽ�ꍇ�̏��������s
            // ���̗�ł́A�A�N�V����1���s��ꂽ�����ƃt���O���L�^
            Debug.Log("�A�N�V����1���N����");
            action1Performed = true;
            action1Timestamp = Time.time;
        }

        // 2. �A�N�V����1���s���Ă���2�b�ȓ��ŁA����������������Ă���ꍇ�A�A�N�V����2�ɐ��ڂ���
        if (action1Performed && (Time.time - action1Timestamp) <= 1f && currentRotationY < 357f && currentRotationY > 300f)
        {
            // �A�N�V����2�ւ̐��ڏ����������ɋL�q
            TransitionToAction2();
        }
    }

    void TransitionToAction2()
    {
        // �A�N�V����2�ւ̐��ڏ����������ɋL�q
        Debug.Log("�A�N�V�����Q�����s");

        // ���ڌ�ɕK�v�ȏ�����ǉ��ł��܂�
        // �Ⴆ�΁A�t���O��^�C���X�^���v�����Z�b�g����Ȃ�
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
