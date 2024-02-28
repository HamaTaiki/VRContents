using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    public struct GoogleTextToSpeechResponse
    {
        public string audioContent;
    }

    [Serializable]
    public struct Input
    {
        public string text;
    }

    [Serializable]
    public struct Voice
    {
        public string languageCode;
        public string name;
        public string ssmlGender;

    }

    [Serializable]
    public struct AudioConfig
    {
        public string audioEncoding;
        //public string pitch;
        //public string speakingRate;
    }

    [Serializable]
    public struct Root
    {
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;
    }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private string _apiKey = "AIzaSyDU29RUQ6woDkSntqz3N7nlEGmCW4gdVd8";

    private static readonly string s_apiUrl = "https://texttospeech.googleapis.com/v1/text:synthesize";

    public async UniTask<AudioClip> Request(string serif)
    {
        Root root = new Root
        {
            input = new Input
            {
                text = serif,
            },
            voice = new Voice
            {
                languageCode = "ja-JP",
                name = "ja-JP-Neural2-B",
                ssmlGender = "FEMALE",
            },
            audioConfig = new AudioConfig
            {
                audioEncoding = "MP3",
                //pitch = 1.6,
                //speakingRate = 1,
            }
        };

        string json = JsonUtility.ToJson(root);

        using var request = new UnityWebRequest($"{s_apiUrl}?key={_apiKey}", "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer(),
        };

        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            throw new Exception();
        }

        string responseString = request.downloadHandler.text;

        GoogleTextToSpeechResponse response = JsonUtility.FromJson<GoogleTextToSpeechResponse>(responseString);

        return await GetMedia(response.audioContent);
    }

    private async UniTask<AudioClip> GetMedia(string base64Message)
    {
        byte[] audioBytes = Convert.FromBase64String(base64Message);
        string tempPath = $"{Application.persistentDataPath}/tmpMP3Base64.mp3";

        await File.WriteAllBytesAsync(tempPath, audioBytes);

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{tempPath}", AudioType.MPEG);

        await request.SendWebRequest();

        if (request.result.Equals(UnityWebRequest.Result.ConnectionError))
        {
            Debug.LogError(request.error);
            return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        File.Delete(tempPath);

        return clip;
    }
}