using System;
using System.Collections.Generic;
using System.IO;
using Core;
using FMOD;

namespace CYAN4S
{
    public class AudioManager : Singleton<AudioManager>
    {
        public FMOD.System system;
        public List<AudioDriver> drivers = new();
        public List<uint> bufferSizes = new() { 32, 64, 128, 256, 512 };
        public int currentDriver = 0;
        public int currentBufferSize = 0;

        [Serializable]
        public struct AudioDriver
        {
            public OUTPUTTYPE type;
            public int id;
            public string name;
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            system = FMODUnity.RuntimeManager.CoreSystem;

            SearchDrivers();
            system.setOutput(drivers[currentDriver].type);
            system.setDriver(drivers[currentDriver].id);
        }

        private void SearchDrivers()
        {
            var outputTypes = new[]
            {
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                OUTPUTTYPE.WASAPI, OUTPUTTYPE.ASIO
                #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
                OUTPUTTYPE.COREAUDIO
                #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                OUTPUTTYPE.PULSEAUDIO
                #elif UNITY_ANDROID
                OUTPUTTYPE.AAUDIO
                #elif UNITY_WEBGL
                FMOD_OUTPUTTYPE_AUDIOWORKLET
                #endif
            };

            foreach (var outputType in outputTypes)
            {
                if (system.setOutput(outputType) != RESULT.OK)
                {
                    continue;
                }

                system.getNumDrivers(out var driver);
                for (var i = 0; i < driver; i++)
                {
                    system.getDriverInfo(i, out var dec, 100, out _, out _, out _, out _);
                    drivers.Add(new AudioDriver { id = i, name = dec, type = outputType });
                }
            }
        }

        public void ChangeDevice(int index)
        {
            system.setOutput(drivers[index].type);
            system.setDriver(drivers[index].id);
            currentDriver = index;
        }

        public void ChangeSize(int index)
        {
            ChangeSize(bufferSizes[index]);
        }

        public void ChangeSize(uint size)
        {
            system.setDSPBufferSize(size, 2);
        }
        
        public static Sound? PrepareSound(string path, string rootPath)
        {
            if (path is null or "")
            {
                return null;
            }

            var system = FMODUnity.RuntimeManager.CoreSystem;
            system.createSound(Path.Combine(rootPath, path), MODE.LOOP_OFF, out var sound);
            return sound;
        }

        public static Channel PlaySound(Sound sound, bool paused = false)
        {
            var system = FMODUnity.RuntimeManager.CoreSystem;
            system.playSound(sound, new ChannelGroup(), paused, out var channel);
            return channel;
        }
    }
}