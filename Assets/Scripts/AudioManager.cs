using System.Collections;
using System.IO;
using NAudio.Wave;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace CYAN4S
{
    public class AudioManager : MonoBehaviour
    {
        public void PlaySoundNAudio()
        {
            var outputDevice = new WaveOutEvent();
            var audioFile = new AudioFileReader(@"C:/Temp/clap.wav");
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        public static IEnumerator GetAudioClip(string audioPath, UnityAction<AudioClip> callback)
        {
            var audioType = Path.GetExtension(audioPath) switch
            {
                ".wav" => AudioType.WAV,
                ".aif" => AudioType.AIFF,
                ".aiff" => AudioType.AIFF,
                ".mp2" => AudioType.MPEG,
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };

            using var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, audioType);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error + " / " + audioPath);
            }
            else
            {
                callback?.Invoke(DownloadHandlerAudioClip.GetContent(www));
            }
        }
    }
}