using System;
using System.IO;
using NAudio.Wave;
using NAudio;
using UnityEngine;

namespace CYAN4S
{
    public class AudioManager : MonoBehaviour
    {
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFile;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        public void Init(string path)
        {
            if (path is null) return;
            
            _outputDevice = new WaveOutEvent();
            _audioFile = new AudioFileReader(@"C:/Temp/clap.wav");
            _outputDevice.Init(_audioFile);
        }

        public void CleanUp()
        {
            _outputDevice.Dispose();
            _outputDevice = null;
            _audioFile.Dispose();
            _audioFile = null;
        }
        
        public void Play()
        {
            _outputDevice?.Play();
        }

        public void Stop()
        {
            _outputDevice?.Stop();
        }

        public static void PlaySoundNAudio()
        {
            var output = new WaveOutEvent();
            var file = new AudioFileReader(@"C:/Temp/clap.wav");
            output.Init(file);
            output.Play();
        }

        public static void PlaySoundNAudio(string path)
        {
            if (path is null) return;

            var output = new WaveOutEvent();
            var file = new AudioFileReader(Path.Combine(Application.dataPath, "Tracks", path));
            output.Init(file);
            output.Play();
        }

        public static void GetDevices()
        {
            
            // for (int n = -1; n < WaveOut.DeviceCount; n++)
            // {
            //     var caps = WaveOut.GetCapabilities(n);
            //     Console.WriteLine($"{n}: {caps.ProductName}");
            // }
        }
    }
}