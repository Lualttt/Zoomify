using System;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using qol_core;
using UnityEngine;
using System.Collections.Generic;

namespace zoomify
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        static Plugin instance;

        ConfigEntry<string> zoomKey;
        ConfigEntry<int> zoomAmount;
        ConfigEntry<float> zoomDuration;

        public static Camera camera;
        public static float scrollZoom;

        public override void Load()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));

            instance = this;
            zoomKey = Config.Bind<string>(
                "Zoom",
                "zoomKey",
                "c",
                "The key that is pressed when you want to zoom in."
                );
            zoomAmount = Config.Bind<int>(
                "Zoom",
                "zoomAmount",
                25,
                "How much you zoom in, lower is more zoom."
                );
            zoomDuration = Config.Bind<float>(
                "Zoom",
                "zoomDuration",
                0.3f,
                "How long it takes to zoom in (0 = never, 1 = instantly)"
                );

            Mods.RegisterMod(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION, "simple zoom mod");
            Commands.RegisterCommand("zoom", "/zoom (amount) (duration) (key)", "Change the zoom settings.", ZoomCommand);

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(MoveCamera), nameof(MoveCamera.Start))]
        [HarmonyPostfix]
        public static void Start(MoveCamera __instance)
        {
            camera = __instance.mainCam;
        }

        [HarmonyPatch(typeof(MoveCamera), nameof(MoveCamera.LateUpdate))]
        [HarmonyPostfix]
        public static void Update(PlayerMovement __instance)
        {
            if (Input.GetKey(instance.zoomKey.Value))
            {
                scrollZoom -= Input.GetAxisRaw("Mouse ScrollWheel") * 20;
                camera.fieldOfView = Mathf.Clamp(Mathf.Lerp(camera.fieldOfView, instance.zoomAmount.Value + scrollZoom, instance.zoomDuration.Value), 1, 120);
            } else
            {
                scrollZoom = 0f;
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, CurrentSettings.Instance.fov, instance.zoomDuration.Value);
            }
        }

        public static bool ZoomCommand(List<string> arguments)
        {
            if (arguments.Count == 1)
            {
                qol_core.Plugin.SendMessage($"zoom: ({instance.zoomAmount.Value}, {instance.zoomDuration.Value}, {instance.zoomKey.Value})");
            } else
            {
                try {
                    if (arguments.Count >= 2)
                    {
                        instance.zoomAmount.Value = int.Parse(arguments[1]);
                    }
                    if (arguments.Count >= 3)
                    {
                        instance.zoomDuration.Value = float.Parse(arguments[2]);
                    }
                    if (arguments.Count >= 4)
                    {
                        instance.zoomKey.Value = arguments[3];
                    }
                    instance.Config.Save();
                    qol_core.Plugin.SendMessage($"zoom: ({instance.zoomAmount.Value}, {instance.zoomDuration.Value}, {instance.zoomKey.Value})");
                } catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}