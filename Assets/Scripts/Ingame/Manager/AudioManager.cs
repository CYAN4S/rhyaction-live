using System.IO;
using NAudio.Wave;
using UnityEngine;

namespace CYAN4S
{
    public class AudioManager : MonoBehaviour
    {
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
    }
}