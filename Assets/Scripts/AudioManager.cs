using NAudio.Wave;
using UnityEngine;

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