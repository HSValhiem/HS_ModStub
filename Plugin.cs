using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using Object = System.Object;

namespace ModStub
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ModStub : BaseUnityPlugin
    {
        public const string ModGUID = "hs.modstub";
        public const string ModName = "HS_ModStub";

        // Change this in Project Settings and also in Thunderstore Manifest
        // Dont forget to Update the Changelog too
        public const string ModVersion = "0.1.0";

        // Change MinimumRequiredVersion to the new version when changing config settings.
        public static ConfigSync configSync = new(ModGUID)
        {
            DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = "0.1.0", ModRequired = true
        };

        // Config Setting Defines
        private static ConfigEntry<Toggle> serverConfigLocked = null!;
        private static ConfigEntry<bool> modEnabled = null!;

        #region Config Boilerplate

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private enum Toggle
        {
            On = 1,
            Off = 0
        }

        #endregion

        public void Awake()
        {
            // Actual Config Settings
            serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            configSync.AddLockingConfigEntry(serverConfigLocked);
            modEnabled = config("1 - General", "Mod Enabled", true, "");

            // Harmony Setup
            Harmony harmony = new Harmony(ModGUID);

            // Harmony Patches can be done in three ways:

            // Patch Everything (Cannot be used with anything else
            //harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Patch a Set of Patches in a Class
            //harmony.PatchAll(typeof(HS_ModStub_Patches));

            // Example Single Harmony Patch for Protected/Internal Methods and Transpilers
            //public Object LoadAsset(string name, Type type)
            harmony.Patch(
                AccessTools.Method(typeof(AssetBundle), "LoadAsset", new[] { typeof(string), typeof(Type) }), // Target Method to Patch (ClassName, MethodName, Argument Types)
                prefix: new HarmonyMethod(typeof(HS_ModStub_Patches), nameof(HS_ModStub_Patches.HS_PatchAssetBundleLoadAsset)) // Patch Method Prefix/Postfix/Transpiler (ClassName, MethodName)
            );


        }

        // Example of how to Define and External and Call in a patch
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern AssetBundle LoadFromFile_Internal(string path, uint crc, ulong offset);

        // Example Inline Patch, this would be picked up by harmony.PatchAll(Assembly.GetExecutingAssembly());
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadFromFile")]
        public class AssetBundle_LoadFromFile_Patch
        {
            public static void Prefix(ref bool __runOriginal, ref AssetBundle __result, string path)
            {
                __runOriginal = false;
                __result = LoadFromFile_Internal(path, 0U, 0UL);
            }
        }

        public static class HS_ModStub_Patches
        {
            // Example Patch
            // __instance is the Class of the Method except for static methods
            // __result is type of original method result and can be set
            // __runOriginal sets if the original method should run
            public static void HS_PatchAssetBundleLoadAsset(AssetBundle __instance, ref bool __runOriginal, ref Object __result, string? name, Type? type)
            {
                // Run Original (Only avail in Prefix)
                __runOriginal = false;

                // Original Code for Example
                if (name == null)
                {
                    throw new NullReferenceException("The input asset name cannot be null.");
                }
                if (name?.Length == 0)
                {
                    throw new ArgumentException("The input asset name cannot be empty.");
                }
                if (type == null)
                {
                    throw new NullReferenceException("The input type cannot be null.");
                }
                // Return Result
                __result = __instance.LoadAsset_Internal(name, type);
            }
        }
    }
}
