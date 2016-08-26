﻿using Engine.Assets;
using Engine.Behaviors;
using Engine.Editor;
using Engine.Graphics;
using Engine.Physics;
using Engine.ProjectSystem;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Veldrid.Assets;
using Veldrid.Platform;

namespace Engine
{
    public static class Program
    {
        public static int Main()
        {
            ProjectManifest projectManifest;
            string currentDir = AppContext.BaseDirectory;
            string manifestName = null;
            foreach (var file in Directory.EnumerateFiles(currentDir))
            {
                if (file.EndsWith("manifest"))
                {
                    if (manifestName != null)
                    {
                        throw new InvalidOperationException("Error: Multiple project manifests in this directory: " + currentDir);
                    }

                    manifestName = file;
                }
            }

            using (var fs = File.OpenRead(manifestName))
            using (var sr = new StreamReader(fs))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                try
                {
                    projectManifest = js.Deserialize<ProjectManifest>(jtr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error was encountered while loading the project manifest.");
                    Console.WriteLine(e);
                    return -1;
                }
            }

            Game game = new Game();

            OpenTKWindow window = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? (OpenTKWindow)new DedicatedThreadWindow() : new SameThreadWindow();
            window.Title = "ge.Main";
            window.Visible = true;
            GraphicsSystem gs = new GraphicsSystem(window);
            game.SystemRegistry.Register(gs);
            window.Closed += game.Exit;

            InputSystem inputSystem = new InputSystem(window);
            inputSystem.RegisterCallback((input) =>
            {
                if (input.GetKeyDown(Key.F4) && (input.GetKey(Key.AltLeft) || input.GetKey(Key.AltRight)))
                {
                    game.Exit();
                }
                if (input.GetKeyDown(Key.F11))
                {
                    window.WindowState = window.WindowState == WindowState.Normal ? WindowState.FullScreen : WindowState.Normal;
                }
            });
            game.SystemRegistry.Register(inputSystem);

            ImGuiRenderer imGuiRenderer = new ImGuiRenderer(gs.Context, window.NativeWindow, inputSystem);
            gs.AddFreeRenderItem(imGuiRenderer);

            AssetSystem assetSystem = new AssetSystem(Path.Combine(AppContext.BaseDirectory, projectManifest.AssetRoot));
            game.SystemRegistry.Register(assetSystem);

            BehaviorUpdateSystem bus = new BehaviorUpdateSystem(game.SystemRegistry);
            game.SystemRegistry.Register(bus);
            bus.Register(imGuiRenderer);

            PhysicsSystem ps = new PhysicsSystem();
            game.SystemRegistry.Register(ps);

#if DEBUG
            ConsoleCommandSystem ccs = new ConsoleCommandSystem(game.SystemRegistry);
            game.SystemRegistry.Register(ccs);
#endif

            SceneAsset scene;
            AssetID mainSceneID = projectManifest.OpeningScene;
            if (mainSceneID.IsEmpty)
            {
                var scenes = assetSystem.Database.GetAssetsOfType(typeof(SceneAsset));
                if (!scenes.Any())
                {
                    Console.WriteLine("No scenes were available to load.");
                    return -1;
                }
                else
                {
                    mainSceneID = scenes.First();
                }
            }

            scene = assetSystem.Database.LoadAsset<SceneAsset>(mainSceneID);
            scene.GenerateGameObjects();

            game.RunMainLoop();

            return 0;
        }
    }
}
