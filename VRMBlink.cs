using UnityEngine;
using System.Collections;
using VRM;

class VRMBlink : MonoBehaviour
{

    [SerializeField] bool isActive = true;              //�I�[�g�ڃp�`�L��
    [SerializeField] SkinnedMeshRenderer FaceMesh;  //EYE_DEF�ւ̎Q��
    [SerializeField] int EyeCloseNum = 0;
    [SerializeField] float ratio_Close = 95.0f;         //���ڃu�����h�V�F�C�v�䗦
    [SerializeField] float ratio_HalfClose = 30.0f;     //�����ڃu�����h�V�F�C�v�䗦
    [HideInInspector]
    [SerializeField] float ratio_Open = 0.0f;
    private bool timerStarted = false;          //�^�C�}�[�X�^�[�g�Ǘ��p
    private bool isBlink = false;               //�ڃp�`�Ǘ��p

    [SerializeField] float timeBlink = 0.4f;                //�ڃp�`�̎���
    private float timeRemining = 0.0f;          //�^�C�}�[�c�莞��

    [SerializeField] float threshold = 0.3f;                // �����_�������臒l
    [SerializeField] float interval = 3.0f;             // �����_������̃C���^�[�o��

    [SerializeField] VRMBlendShapeProxy _blendShapeProxy;

    enum Status
    {
        Close,
        HalfClose,
        Open    //�ڃp�`�̏��
    }

    private Status eyeStatus;   //���݂̖ڃp�`�X�e�[�^�X

    // Use this for initialization
    void Start()
    {
        ResetTimer();
        // �����_������p�֐����X�^�[�g����
        StartCoroutine("RandomChange");
    }

    //�^�C�}�[���Z�b�g
    void ResetTimer()
    {
        timeRemining = timeBlink;
        timerStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        {
            eyeStatus = Status.Close;
            timerStarted = true;
        }
        if (timerStarted)
        {
            timeRemining -= Time.deltaTime;
            if (timeRemining <= 0.0f)
            {
                eyeStatus = Status.Open;
                ResetTimer();
            }
            else if (timeRemining <= timeBlink * 0.3f)
            {
                eyeStatus = Status.HalfClose;
            }
        }
    }

    void LateUpdate()
    {
        float JoyValue = _blendShapeProxy.GetValue(BlendShapePreset.Joy);
        float FunValue = _blendShapeProxy.GetValue(BlendShapePreset.Fun);
        float AngryValue = _blendShapeProxy.GetValue(BlendShapePreset.Angry);

        if (JoyValue == 1 || AngryValue == 1 || FunValue == 1)
        {
            isActive = false;
        }



        if (isActive)
        {
            if (isBlink)
            {
                switch (eyeStatus)
                {
                    case Status.Close:
                        SetCloseEyes();
                        break;
                    case Status.HalfClose:
                        SetHalfCloseEyes();
                        break;
                    case Status.Open:
                        SetOpenEyes();
                        isBlink = false;
                        break;
                }
                //Debug.Log(eyeStatus);
            }
        }

        isActive = true;
    }

    void SetCloseEyes()
    {
        FaceMesh.SetBlendShapeWeight(EyeCloseNum, ratio_Close);
    }

    void SetHalfCloseEyes()
    {
        FaceMesh.SetBlendShapeWeight(EyeCloseNum, ratio_HalfClose);
    }

    void SetOpenEyes()
    {
        FaceMesh.SetBlendShapeWeight(EyeCloseNum, ratio_Open);
    }

    // �����_������p�֐�
    IEnumerator RandomChange()
    {
        // �������[�v�J�n
        while (true)
        {
            //�����_������p�V�[�h����
            float _seed = Random.Range(0.0f, 1.0f);
            if (!isBlink)
            {
                if (_seed > threshold)
                {
                    isBlink = true;
                }
            }
            // ���̔���܂ŃC���^�[�o����u��
            yield return new WaitForSeconds(interval);
        }
    }
}