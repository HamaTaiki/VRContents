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
    private const string MicName = null; //�}�C�N�f�o�C�X�̖��O
    private const int SamplingFrequency = 44100; //�T���v�����O���g��
    private const int MaxTimeSeconds = 10; //�ő�^������[s]

    private bool _isRecording = false;
    private bool _isCooldown = false;
    private float _cooldownDuration = 3f; // �N�[���_�E���̊��ԁi�b�j

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
        _recordingText.text = "������蒆...";

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
        _recordingText.text = " ��~�I";
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

        //ChatGPT�ɑ��M
        string name = "����: Json�`���Řb���� "+ responseText;
        string name2 = responseText;

        //�󔒂ɂ���
        //_inputField.text = "";

        //���M�e�L�X�g��\������
        _displayText.text += "User: " + name2;

         
        var responseObject = await chatGPTConnection.RequestAsync(name);

        //Json�ɕύX
        string returnData = responseObject.choices[0].message.content;

        if (returnData.Substring(0, 1) == "{")
        {
            JObject Json = JObject.Parse(returnData);

            ////Json���璊�o
            string message = Json["message"].ToString();
            string joyString = Json["emotion"]["joy"].ToString();
            string fun = Json["emotion"]["fun"].ToString();
            string anger = Json["emotion"]["anger"].ToString();
            string sad = Json["emotion"]["sad"].ToString();

            ////����l�𐮐��ɕϊ�
            int Joy = int.Parse(joyString);
            int Fun = int.Parse(fun);
            int Anger = int.Parse(anger);
            int Sad = int.Parse(sad);


            //�����Đ��ƕ\��ύX
            AudioClip clip = await _textToSpeech.Request(message);

            // AudioClip���擾���AAudioSource�ɑ��
            _audioSource.clip = clip;

            //�I�[�f�B�I�̒������擾
            int audiolength = (int)_audioSource.clip.length;

            // AudioSource�ōĐ�
            _audioSource.Play();

            //�\��ύX
            ChangeFace(Joy, Fun, Anger, Sad);

            //�I�[�f�B�I�̒���������~���Ă���\��Z�b�g
            Invoke("ResetFace", audiolength + 1);

            //�ԓ����e�L�X�g�ɕ\��
            // �e�L�X�g��\��
            _displayText.text += "\n";
            _displayText.text += "Miru: " + message;
            _displayText.text += "\n";

            _isRecording = false;
        }
        else
        {
            _recordingText.text += "�ԓ��̎擾���s";
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
    /// �������g�p���̃}�C�N����肷��ۂɗ��p
    /// </summary>
    private void ShowAllMicDevices()
    {
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}