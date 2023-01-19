using System;
using System.IO;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CYAN4S
{
    public class AudioManager : MonoBehaviour
    {
        public static Sound? PrepareSound(string path)
        {
            if (path is null or "")
            {
                return null;
            }
            
            var system = FMODUnity.RuntimeManager.CoreSystem;
            var fullPath = Path.Combine(Application.dataPath, "Tracks");
            system.createSound(Path.Combine(fullPath, path), MODE.DEFAULT, out var sound);
            return sound;
        }

        public static Channel PlaySound(Sound sound)
        {
            var system = FMODUnity.RuntimeManager.CoreSystem;
            system.playSound(sound, new ChannelGroup(), false, out var channel);
            return channel;
        }

    }
}