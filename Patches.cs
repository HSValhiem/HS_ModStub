using HarmonyLib;

namespace HS_ModStub;

// Put your Harmony patches here (or in more files). Plugin.Awake applies every [HarmonyPatch] class with PatchAll.
// Check Plugin.ModEnabled.Value in each patch so the "Mod Enabled" setting works.
[HarmonyPatch(typeof(Game), nameof(Game.Start))]
internal static class GameStartPatch
{
	private static void Postfix()
	{
		if (!Plugin.ModEnabled.Value) return;
		Plugin.Log.LogInfo($"{BuildInfo.Name} {BuildInfo.Version} loaded into the game.");
	}
}
