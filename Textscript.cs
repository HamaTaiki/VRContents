using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AAA.OpenAI;
using Newtonsoft.Json.Linq;
using VRM;
using System.Threading;
using Cysharp.Threading.Tasks;
using Audio;
using WhisperAPI;
using WhisperAPI.Models;





public class Textscript : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private VRMBlendShapeProxy proxy;
    [SerializeField] private TextMeshProUGUI _displayText;
    [SerializeField] private TextMeshProUGUI _recordingText;
    [SerializeField] private TextMeshProUGUI _countdownText;

    private AudioClip _recordedClip;
    private const string MicName = null; //マイクデバイスの名前
    private const int SamplingFrequency = 44100; //サンプリング周波数
    private const int MaxTimeSeconds = 10; //最大録音時間[s]

    private bool _isRecording = false;
    private bool _isCooldown = false;
    private float _cooldownDuration = 3f; // クールダウンの期間（秒）

    private WhisperAPIConnection _whisperConnection;

    private CancellationTokenSource _cts = new();
    private CancellationToken _token;

    TMP_InputField _inputField;
    ChatGPTConnection chatGPTConnection;
    TextToSpeech _textToSpeech;

    void Start()
    {
        ///_inputField = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        _token = _cts.Token;
        _whisperConnection = new(Constants.API_KEY);
        chatGPTConnection = new ChatGPTConnection(Constants.API_KEY);
        _textToSpeech = new TextToSpeech();
        _displayText = GameObject.Find("Content").GetComponent<TextMeshProUGUI>();
        _recordingText = GameObject.Find("RecordingText").GetComponent<TextMeshProUGUI>();
        _countdownText = GameObject.Find("CountDown").GetComponent<TextMeshProUGUI>();

        ResetFace();
        ShowAllMicDevices();
    }

    void Update()
    {
        if (HandCheck.isAction2Executed && !_isRecording || OVRInput.GetDown(OVRInput.Button.Two))
        {

            _recordingText.text = "";
            StartCoroutine(RecordingCoroutine());
        }
    }

    IEnumerator RecordingCoroutine()
    {
        // Start recording
        _displayText.text = "";
        StartRecording();
        _recordingText.text = "聞き取り中...";

        // Wait for 3 seconds
        yield return new WaitForSeconds(1);
        _countdownText.text = "3";
        yield return new WaitForSeconds(1);
        _countdownText.text = "2";
        yield return new WaitForSeconds(1);
        _countdownText.text = "1";
        yield return new WaitForSeconds(1);
        _countdownText.text = "";


        // Stop recording after 3 seconds
        _recordingText.text = "";
        StopRecording();
        _recordingText.text = " 停止！";
        yield return new WaitForSeconds(2);
        _recordingText.text = "";

        _isCooldown = true;
        yield return new WaitForSeconds(_cooldownDuration);
        _isCooldown = false;


    }

    private void StartRecording()
    {
        Debug.Log("recording start");
        _isRecording = true;
        _recordedClip = Microphone.Start(deviceName: MicName, loop: false, lengthSec: MaxTimeSeconds, frequency: SamplingFrequency);

    }

    private async void StopRecording()
    {
        if (Microphone.IsRecording(deviceName: MicName))
        {
            Debug.Log("recording stopped");
            Microphone.End(deviceName: MicName);
        }
        else
        {
            Debug.Log("not recording");
            return;
        }

        byte[] recordWavData = WavConverter.ToWav(_recordedClip);

        // WhisperAPI
        string responseText = await DisplayWhisperResponse(recordWavData);

        // ChatGPTAPI
        //DisplayChatGPTResponse(responseText);

        //ChatGPTに送信
        string name = "命令: Json形式で話して "+ responseText;
        string name2 = responseText;

        //空白にする
        //_inputField.text = "";

        //送信テキストを表示する
        _displayText.text += "User: " + name2;

         
        var responseObject = await chatGPTConnection.RequestAsync(name);

        //Jsonに変更
        string returnData = responseObject.choices[0].message.content;

        if (returnData.Substring(0, 1) == "{")
        {
            JObject Json = JObject.Parse(returnData);

            ////Jsonから抽出
            string message = Json["message"].ToString();
            string joyString = Json["emotion"]["joy"].ToString();
            string fun = Json["emotion"]["fun"].ToString();
            string anger = Json["emotion"]["anger"].ToString();
            string sad = Json["emotion"]["sad"].ToString();

            ////感情値を整数に変換
            int Joy = int.Parse(joyString);
            int Fun = int.Parse(fun);
            int Anger = int.Parse(anger);
            int Sad = int.Parse(sad);


            //音声再生と表情変更
            AudioClip clip = await _textToSpeech.Request(message);

            // AudioClipを取得し、AudioSourceに代入
            _audioSource.clip = clip;

            //オーディオの長さを取得
            int audiolength = (int)_audioSource.clip.length;

            // AudioSourceで再生
            _audioSource.Play();

            //表情変更
            ChangeFace(Joy, Fun, Anger, Sad);

            //オーディオの長さだけ停止してから表情リセット
            Invoke("ResetFace", audiolength + 1);

            //返答をテキストに表示
            // テキストを表示
            _displayText.text += "\n";
            _displayText.text += "Miru: " + message;
            _displayText.text += "\n";

            _isRecording = false;
        }
        else
        {
            _recordingText.text += "返答の取得失敗";
            _isRecording = false;
        }

        
    }

    private async UniTask<string> DisplayWhisperResponse(byte[] recordData)
    {
        WhisperAPIResponseModel responseModel = await _whisperConnection.RequestWithVoiceAsync(recordData, _token);
        return responseModel.text;
    }

    private void ResetFace()
    {
        proxy.AccumulateValue(BlendShapePreset.Neutral, 0);
        proxy.AccumulateValue(BlendShapePreset.Joy, 0);
        proxy.AccumulateValue(BlendShapePreset.Fun, 0);
        proxy.AccumulateValue(BlendShapePreset.Angry, 0);
        proxy.AccumulateValue(BlendShapePreset.Sorrow, 0);
        proxy.Apply();

    }

    private void ChangeFace(int a, int b, int c, int d)
    {
        var maxEmotion = 0;
        BlendShapePreset preset = BlendShapePreset.Neutral;
        if (a > maxEmotion)
        {
            maxEmotion = a;
            preset = BlendShapePreset.Joy;
        }
        if (b > maxEmotion)
        {
            maxEmotion = b;
            preset = BlendShapePreset.Fun;
        }
        if (c > maxEmotion)
        {
            maxEmotion = c;
            preset = BlendShapePreset.Angry;
        }
        if (d > maxEmotion)
        {
            maxEmotion = d;
            preset = BlendShapePreset.Sorrow;
        }

        proxy.AccumulateValue(preset, 1);
        proxy.Apply();
    }

    /// <summary>
    /// 自分が使用中のマイクを特定する際に利用
    /// </summary>
    private void ShowAllMicDevices()
    {
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}