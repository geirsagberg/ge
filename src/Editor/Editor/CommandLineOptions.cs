using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Engine.Editor
{
    public class CommandLineOptions
    {
        private bool _preferOpenGL = EditorPreferences.Instance.PreferOpenGL;
        private string _project;
        private string _scene;
        private AudioEnginePreference? _audioPreference;

        public bool PreferOpenGL => _preferOpenGL;
        public string Project => _project;
        public string Scene => _scene;
        public AudioEnginePreference? AudioPreference => _audioPreference;

        public CommandLineOptions(string[] args)
        {
            var rootCommand = new RootCommand {
                new Option<bool>("--opengl", description: "Prefer using the OpenGL rendering backend."),
                new Option<string>(new[] { "--project", "-p" }, "Specifies the project to open."),
                new Option<string>(new[] { "--scene", "-s" }, "Specifies the scene to open."),
                new Option<AudioEnginePreference>("audio", "Specifies the audio engine to use.")
            };
            rootCommand.Handler = CommandHandler.Create<bool, string, string, AudioEnginePreference>(
                (preferOpenGL, project, scene, audioPreference) => {
                    _preferOpenGL = preferOpenGL;
                    _project = project;
                    _scene = scene;
                    _audioPreference = audioPreference;
                });
            rootCommand.Invoke(args);
        }

        public enum AudioEnginePreference
        {
            OpenAL,
            None
        }
    }
}
