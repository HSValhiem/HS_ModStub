using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HS;
using ItemManager;
using LocalizationManager;
using ServerSync;
using UnityEngine;
using Object = System.Object;

namespace HS_ModStub;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	// Change MinimumRequiredVersion to the new version when changing config settings.
	public static ConfigSync configSync = new(MyPluginInfo.PLUGIN_GUID)
	{
		DisplayName = MyPluginInfo.PLUGIN_NAME, CurrentVersion = MyPluginInfo.PLUGIN_VERSION, MinimumRequiredVersion = "0.0.1", ModRequired = true
	};

	// Config Setting Defines
	private static ConfigEntry<Toggle> serverConfigLocked = null!;
	private static ConfigEntry<bool> modEnabled = null!;

	// Localization Examples
    private static ConfigEntry<float> warlockHatSmokeScreenSizeIncrease = null!;
    private static ConfigEntry<int> warlockHatSmokeScreenBlockIncrease = null!;
    private static ConfigEntry<int> warlockHatSmokeScreenDurationIncrease = null!;

	// Logger
    public static readonly ManualLogSource HS_Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_NAME);

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

	// Full List of Default MonoBehavior Methods like Awake
	//void Awake() is called prior to Start()
	//void Start() is called is the GameObject is enabled
	//void Update() is called every frame, can be skipped for several frames to keep FPS up
	//void FixedUpdate() is called every frame, will not be skipped
	//void LateUpdate() is called after all other Update-methods
	//void OnEnable()/OnDiable() is called when the GameObject is enabled/disabled
	//void OnDestroy() is called when the GameObject is destroyed(via GameObject.Destroy)
	//void OnGUI() is called when drawing and allows the script to use the GUI-API(described later on)
	public void Awake()
	{
        // Check if Plugin was Built for Current Version of Valheim
        if (!VersionChecker.Check(HS_Logger, Info, false, Config)) return;

        Localizer.Load(); // Use this to initialize the LocalizationManager

        // Actual Config Settings
        serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
		configSync.AddLockingConfigEntry(serverConfigLocked);
		modEnabled = config("1 - General", "Mod Enabled", true, "");

        // Localization Examples
        warlockHatSmokeScreenSizeIncrease = Config.Bind("Odins Warlock Hat", "Smoke Screen Size Increase", 2f, new ConfigDescription("Radius increase for the smoke screen ability of the Dragon Staff while wearing the Warlock hat.", new AcceptableValueRange<float>(0f, 5f)));
        warlockHatSmokeScreenDurationIncrease = Config.Bind("Odins Warlock Hat", "Smoke Screen Duration Increase", 120, new ConfigDescription("Duration increase for the smoke screen ability of the Dragon Staff while wearing the Warlock hat in seconds.", new AcceptableValueRange<int>(0, 300)));
        warlockHatSmokeScreenBlockIncrease = Config.Bind("Odins Warlock Hat", "Smoke Screen Block Chance Increase", 25, new ConfigDescription("Projectile block chance increase for the smoke screen ability of the Dragon Staff while wearing the Warlock hat.", new AcceptableValueRange<int>(0, 100)));


        Localizer.AddPlaceholder("pp_odins_warlock_hat_description", "power", warlockHatSmokeScreenBlockIncrease); // This will replace the {power} placeholder in your localization string with the value from the warlockHatSmokeScreenBlockIncrease
        Localizer.AddPlaceholder("pp_odins_warlock_hat_description", "radius", warlockHatSmokeScreenSizeIncrease);
        Localizer.AddPlaceholder("pp_odins_warlock_hat_description", "duration", warlockHatSmokeScreenDurationIncrease, duration => (duration / 60f).ToString("0.#")); // There is another parameter you can use to change the representation of a value. This will convert the seconds from the config entry to minutes for the display string

        Item alchemyEquipment = new("potions", "Odins_Warlock_Hat");
        alchemyEquipment.Crafting.Add(CraftingTable.Workbench, 5);
        alchemyEquipment.MaximumRequiredStationLevel = 5;
        alchemyEquipment.RequiredItems.Add("LinenThread", 20);
        alchemyEquipment.RequiredItems.Add("SurtlingCore", 5);
        alchemyEquipment.RequiredUpgradeItems.Add("LinenThread", 10);
        alchemyEquipment.RequiredUpgradeItems.Add("SurtlingCore", 2);

        // Harmony Setup
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

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

