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
        private AudioFileReader audioFile;
        private AudioFileReader audioFileAsio;
        private WaveOutEvent outputDevice;

        // private AudioSource _audioSource;
        private AudioClip _audioClip;
        private AsioOut _asioOut;

        private void Awake()
        {
            // _audioSource = GetComponent<AudioSource>();
        
            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(@"C:/Temp/clap.wav");
            audioFileAsio = new AudioFileReader(@"C:/Temp/hihat.wav");
            outputDevice.Init(audioFile);

            // StartCoroutine(GetAudioClip(@"C:/Temp/clap.wav", arg0 =>
            // {
            //     _audioClip = arg0;
            //     _audioSource.clip = _audioClip;
            // }));

            foreach (var driver in AsioOut.GetDriverNames())
            {
                Debug.Log(driver);
            }

            _asioOut = new AsioOut(AsioOut.GetDriverNames()[1]);
            _asioOut.Init(audioFileAsio);
        }

        private void OnDestroy()
        {
            _asioOut.Dispose();
        }

        public void PlaySoundNAudio()
        {
            outputDevice.Pause();
            audioFile.Position = 0;
            outputDevice.Play();
        }

        public void PlaySoundAsio()
        {
            audioFileAsio.Position = 0;
            _asioOut.Play();
        }

        public void PlaySound()
        {
            // _audioSource.Play();
        }

        public void PlaySoundAsio(string path)
        {
            PlaySoundAsio();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }

        private void Update()
        {
            // if (Keyboard.current.fKey.wasPressedThisFrame)
            // {
            //     PlaySoundNAudio();
            // }
            //
            // if (Keyboard.current.jKey.wasPressedThisFrame)
            // {
            //     PlaySound();
            // }
            //
            // if (Keyboard.current.spaceKey.wasPressedThisFrame)
            // {
            //     PlaySoundAsio();
            // }
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