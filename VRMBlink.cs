using UnityEngine;
using System.Collections;
using VRM;

class VRMBlink : MonoBehaviour
{

    [SerializeField] bool isActive = true;              //オート目パチ有効
    [SerializeField] SkinnedMeshRenderer FaceMesh;  //EYE_DEFへの参照
    [SerializeField] int EyeCloseNum = 0;
    [SerializeField] float ratio_Close = 95.0f;         //閉じ目ブレンドシェイプ比率
    [SerializeField] float ratio_HalfClose = 30.0f;     //半閉じ目ブレンドシェイプ比率
    [HideInInspector]
    [SerializeField] float ratio_Open = 0.0f;
    private bool timerStarted = false;          //タイマースタート管理用
    private bool isBlink = false;               //目パチ管理用

    [SerializeField] float timeBlink = 0.4f;                //目パチの時間
    private float timeRemining = 0.0f;          //タイマー残り時間

    [SerializeField] float threshold = 0.3f;                // ランダム判定の閾値
    [SerializeField] float interval = 3.0f;             // ランダム判定のインターバル

    [SerializeField] VRMBlendShapeProxy _blendShapeProxy;

    enum Status
    {
        Close,
        HalfClose,
        Open    //目パチの状態
    }

    private Status eyeStatus;   //現在の目パチステータス

    // Use this for initialization
    void Start()
    {
        ResetTimer();
        // ランダム判定用関数をスタートする
        StartCoroutine("RandomChange");
    }

    //タイマーリセット
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

    // ランダム判定用関数
    IEnumerator RandomChange()
    {
        // 無限ループ開始
        while (true)
        {
            //ランダム判定用シード発生
            float _seed = Random.Range(0.0f, 1.0f);
            if (!isBlink)
            {
                if (_seed > threshold)
                {
                    isBlink = true;
                }
            }
            // 次の判定までインターバルを置く
            yield return new WaitForSeconds(interval);
        }
    }
}