using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HS;
using LocalizationManager;
using ServerSync;

namespace HS_ModStub;

[BepInPlugin(BuildInfo.Guid, BuildInfo.Name, BuildInfo.Version)]
public class Plugin : BaseUnityPlugin
{
	// Clients must run at least this version to join a server running the mod.
	// Raise it when you add, remove or change synced config settings.
	public static readonly ConfigSync ConfigSync = new(BuildInfo.Guid)
	{
		DisplayName = BuildInfo.Name,
		CurrentVersion = BuildInfo.Version,
		MinimumRequiredVersion = "1.0.0",
		ModRequired = true
	};

	public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource(BuildInfo.Name);

	private static ConfigEntry<Toggle> _serverConfigLocked = null!;
	private static ConfigEntry<bool> _allowWrongValheimVersion = null!;
	internal static ConfigEntry<bool> ModEnabled = null!;

	private Harmony? _harmony;

	public void Awake()
	{
		// Loads translations/*.yml (embedded) and any <ModName>.<Language>.yml next to the DLL.
		Localizer.Load();

		_serverConfigLocked = Bind("1 - General", "Lock Configuration", Toggle.On,
			"If on, the configuration is locked and can be changed by server admins only.");
		ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

		ModEnabled = Bind("1 - General", "Mod Enabled", true, "Turns the mod off without uninstalling it.");

		_allowWrongValheimVersion = Bind("1 - General", "Allow Wrong Valheim Version", true,
			new ConfigDescription(
				$"If on, the mod still starts (with a warning) when the game is not Valheim {BuildInfo.ValheimVersion}. If off, it stays disabled.",
				null, new ConfigurationManagerAttributes { Browsable = false }),
			synchronizedSetting: false);

		if (!VersionChecker.Check(Log, Info, BuildInfo.ValheimVersion, _allowWrongValheimVersion.Value, base.Config)) return;

		// Applies every [HarmonyPatch] class in this assembly (see Patches.cs).
		_harmony = new Harmony(BuildInfo.Guid);
		_harmony.PatchAll(Assembly.GetExecutingAssembly());

#if HS_EXAMPLES
		Examples.ExampleSetup.Run(this);
#endif
	}

	public void OnDestroy() => _harmony?.UnpatchSelf();

	#region Config helpers

	internal ConfigEntry<T> Bind<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
		SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
		syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
		return configEntry;
	}

	internal ConfigEntry<T> Bind<T>(string group, string name, T value, string description, bool synchronizedSetting = true) =>
		Bind(group, name, value, new ConfigDescription(description), synchronizedSetting);

	private enum Toggle
	{
		On = 1,
		Off = 0
	}

	#endregion
}
