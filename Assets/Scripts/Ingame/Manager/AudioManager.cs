using System;
using System.Collections.Generic;
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
        public List<AudioDriver> drivers = new();
        public int current = 0;

        [Serializable]
        public struct AudioDriver
        {
            public OUTPUTTYPE type;
            public int id;
            public string name;
        }

        private void Start()
        {
            if (drivers.Count != 0)
            {
                return;
            }

            system = FMODUnity.RuntimeManager.CoreSystem;
            system.setOutput(OUTPUTTYPE.AUTODETECT);
            
            var types = new[] { OUTPUTTYPE.WASAPI, OUTPUTTYPE.ASIO };

            foreach (var type in types)
            {
                if (system.setOutput(type) != RESULT.OK)
                    continue;

                system.getOutput(out var output);
                if (type != output)
                    continue;
                
                system.getNumDrivers(out var driver);
                for (var i = 0; i < driver; i++)
                {
                    system.getDriverInfo(i, out var dec, 100, out _, out _, out _, out _);
                    drivers.Add(new AudioDriver {id = i, name = dec, type = type});
                }
            }

            Debug.Log("checked!");

            system.setOutput(drivers[current].type);
            system.setDriver(drivers[current].id);
        }

        public void ChangeDevice(int index)
        {
            system.setOutput(drivers[index].type);
            system.setDriver(drivers[index].id);
            current = index;
        }

        public void ChangeSize(uint size)
        {
            system.setDSPBufferSize(size, 2);
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