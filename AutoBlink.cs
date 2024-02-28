using UnityEngine;
using System.Collections;

/// <summary>
/// �����u�����s���N���X�B
/// �g�p����ɂ͓K�؂ɐݒ肳�ꂽAnimatorController������
/// �I�u�W�F�N�g��_Animator�Ɏw�肷��K�v������B
/// </summary>

public class AutoBlink : MonoBehaviour
{
    float weight = 0f;
    float velocity = 0.0f;
    bool isBlink = false;

    [SerializeField]
    float OnceBlinkTime_1 = 3f;
    [SerializeField]
    float OnceBlinkTime_2 = 5f;
    [SerializeField]
    float TwiceBlinkTime_3 = 7f;
    [SerializeField]
    float CloseTime = 0.1f;
    [SerializeField]
    Animator _Animator;

    [SerializeField]
    int Layer = 1;
    // ��������_Animator��Layer�ԍ�
    // VR Motion Recorder�ł�3�Ԃ��܂΂����p��Layer�ƂȂ��Ă���
    // (0, 1, 2, 3, ...�Ɛ����邩��A���ۂ�4�Ԗڂ�Layer)
    // ����_Animator�ɂ���ēK�؂ɐݒ肷�邱��
    // Animator��Layer�@�\�ɂ��Ă͑O�̋L��(�`���ɏЉ���L��)���Q�Ƃ̂���

    void Start()
    {
        // �܂΂������[�v�J�n
        StartCoroutine("AutoBlinkCoroutine");
    }

    // _Animator��weight�𖈃t���[���X�V����
    private void Update()
    {
        if (isBlink)
        {
            // isBlink = true�Ȃ�weight��1�ɂ����Ă���
            // 0.05sec������weight��1�ɂ���
            weight = Mathf.SmoothDamp(weight, 1, ref velocity, 0.05f);
        }
        else
        {
            // isBlink = false�Ȃ�weight��0�ɂ����Ă���
            // 0.05sec������weight��0�ɂ���
            weight = Mathf.SmoothDamp(weight, 0, ref velocity, 0.05f);
        }
        // Animator��Layer�̃E�F�C�g���X�V
        _Animator.SetLayerWeight(Layer, weight);
    }

    //�u�R���[�`���v�ŌĂяo�����\�b�h
    IEnumerator AutoBlinkCoroutine()
    {
        //�������[�v
        while (true)
        {
            //�܂΂���1��ځB�P���ł܂΂���������B
            yield return new WaitForSeconds(OnceBlinkTime_1);
            isBlink = true;
            yield return new WaitForSeconds(CloseTime);
            isBlink = false;

            //�܂΂���2��ځB�P���ł܂΂���������B
            yield return new WaitForSeconds(OnceBlinkTime_2);
            isBlink = true;
            yield return new WaitForSeconds(CloseTime);
            isBlink = false;

            //�܂΂���3��ځB�����ł�2��A���ł܂΂���������B
            yield return new WaitForSeconds(TwiceBlinkTime_3);
            isBlink = true;
            yield return new WaitForSeconds(CloseTime);
            isBlink = false;
            yield return new WaitForSeconds(CloseTime);
            isBlink = true;
            yield return new WaitForSeconds(CloseTime);
            isBlink = false;
        }
    }
}