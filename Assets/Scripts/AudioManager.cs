using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CYAN4S
{
    public class AudioManager : MonoBehaviour
    {
        public void PlaySoundNAudio()
        {
            var output = new WaveOutEvent();
            var file = new AudioFileReader(@"C:/Temp/clap.wav");
            output.Init(file);
            output.Play();
        }
    }
}