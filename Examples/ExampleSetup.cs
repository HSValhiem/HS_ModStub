#if HS_EXAMPLES
// Reference examples, compiled only with -p:IncludeExamples=true (which also fetches ItemManager).
// They are not part of a normal build. Copy what you need into your mod and delete this folder.
using System;
using BepInEx.Configuration;
using HarmonyLib;
using ItemManager;
using LocalizationManager;
using UnityEngine;

namespace HS_ModStub.Examples;

internal static class ExampleSetup
{
	private static ConfigEntry<float> _smokeRadius = null!;
	private static ConfigEntry<int> _smokeDuration = null!;
	private static ConfigEntry<int> _smokeBlock = null!;

	public static void Run(Plugin plugin)
	{
		LocalizationPlaceholders(plugin);
		RegisterItem();
		ManualPatch();
	}

	// Config values shown inside a translated string ({power}, {radius}, {duration} in Examples/translations/English.yml).
	private static void LocalizationPlaceholders(Plugin plugin)
	{
		_smokeRadius = plugin.Bind("Example Item", "Smoke Screen Size Increase", 2f,
			new ConfigDescription("Radius increase of the smoke screen.", new AcceptableValueRange<float>(0f, 5f)));
		_smokeDuration = plugin.Bind("Example Item", "Smoke Screen Duration Increase", 120,
			new ConfigDescription("Duration increase of the smoke screen, in seconds.", new AcceptableValueRange<int>(0, 300)));
		_smokeBlock = plugin.Bind("Example Item", "Smoke Screen Block Chance Increase", 25,
			new ConfigDescription("Projectile block chance increase, in percent.", new AcceptableValueRange<int>(0, 100)));

		Localizer.AddPlaceholder("hs_example_hat_description", "power", _smokeBlock);
		Localizer.AddPlaceholder("hs_example_hat_description", "radius", _smokeRadius);
		// The optional last argument changes how the value is shown: seconds in the config, minutes in the text.
		Localizer.AddPlaceholder("hs_example_hat_description", "duration", _smokeDuration, seconds => (seconds / 60f).ToString("0.#"));
	}

	// ItemManager: an item from an AssetBundle. Needs Examples/assets/<bundle> embedded and a prefab of that name in it;
	// see Examples/assets/README.md. Without the bundle this logs an error and adds nothing.
	private static void RegisterItem()
	{
		Item hat = new("hs_example_bundle", "HS_Example_Hat");
		hat.Crafting.Add(CraftingTable.Workbench, 2);
		hat.MaximumRequiredStationLevel = 5;
		hat.RequiredItems.Add("LinenThread", 20);
		hat.RequiredItems.Add("SurtlingCore", 5);
		hat.RequiredUpgradeItems.Add("LinenThread", 10);
		hat.RequiredUpgradeItems.Add("SurtlingCore", 2);
	}

	// A manual Harmony patch, for targets [HarmonyPatch] can't express (overloads picked at runtime, optional mods).
	private static void ManualPatch()
	{
		Harmony harmony = new(BuildInfo.Guid + ".examples");
		harmony.Patch(
			AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), new[] { typeof(string), typeof(Type) }),
			postfix: new HarmonyMethod(typeof(ExampleSetup), nameof(LoadAssetPostfix)));
	}

	// __result is the original method's return value; a prefix could also take "ref bool __runOriginal" to skip it.
	private static void LoadAssetPostfix(string name, UnityEngine.Object? __result)
	{
		if (__result == null) Plugin.Log.LogDebug($"AssetBundle.LoadAsset found nothing for '{name}'.");
	}
}
#endif
