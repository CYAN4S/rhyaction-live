using System;
using System.IO;
using Core;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CYAN4S
{
    
    public class AudioManager : Singleton<AudioManager>
    {
        public FMOD.System system;
        
        public override void Awake()
        {
            base.Awake();

            Debug.Log("Hi!");
            
            system = FMODUnity.RuntimeManager.CoreSystem;
            system.setOutput(OUTPUTTYPE.ASIO);
            system.getNumDrivers(out var driver);
            for (var i = 0; i < driver; i++)
            {
                system.getDriverInfo(i, out var dec, 100, out var _, out var _, out var _, out var _);
                Debug.Log(dec);
            }

        }

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