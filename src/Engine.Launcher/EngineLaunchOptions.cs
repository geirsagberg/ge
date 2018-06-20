using System;
using System.CommandLine;

namespace Engine
{
    public class EngineLaunchOptions
    {
        private bool _preferOpenGL = false;
        private AudioEnginePreference? _audioPreference;

        public bool PreferOpenGL => _preferOpenGL;
        public AudioEnginePreference? AudioPreference => _audioPreference;

        public EngineLaunchOptions(bool opengl = false, AudioEnginePreference? audio = null)
        {
            
        }

        public enum AudioEnginePreference
        {
            Default,
            OpenAL,
            None
        }
    }
}
