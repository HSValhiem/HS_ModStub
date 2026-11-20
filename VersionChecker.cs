using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;

namespace HS;

// Compares the running game with the Valheim version the mod was built for (ValheimVersion in the .csproj).
internal static class VersionChecker
{
	public static bool Check(ManualLogSource logger, PluginInfo info, string builtFor, bool allowWrongVersion, ConfigFile? config = null)
	{
		string current = global::Version.CurrentVersion.ToString();
		if (current == builtFor) return true;

		string mismatch =
			$"{info.Metadata.Name} v{info.Metadata.Version} was built for Valheim {builtFor}, but this game is Valheim {current}.";

		if (!allowWrongVersion)
		{
			string error = $"ERROR: {mismatch} The mod is disabled. Get the version of the mod made for this game, or wait for an update.";
			logger.LogError(error);
			Chainloader.DependencyErrors.Add(error);
			if (config != null) SetConfigAlert(config, error);
			return false;
		}

		logger.LogWarning($"WARNING: {mismatch} It starts anyway because 'Allow Wrong Valheim Version' is on; report problems with both version numbers.");
		return true;
	}

	private static void SetConfigAlert(ConfigFile config, string errorMessage)
	{
		config.Bind("", "NotCompatible", "", new ConfigDescription(
			errorMessage, null, new ConfigurationManagerAttributes
			{
				CustomDrawer = ErrorLabelDrawer,
				ReadOnly = true,
				HideDefaultButton = true,
				HideSettingName = true,
				Category = null
			}
		));
	}

	private static void ErrorLabelDrawer(ConfigEntryBase entry)
	{
		GUIStyle styleNormal = new(GUI.skin.label) { wordWrap = true, stretchWidth = true };
		GUIStyle styleError = new(GUI.skin.label)
		{
			stretchWidth = true,
			alignment = TextAnchor.MiddleCenter,
			normal = { textColor = Color.red },
			fontStyle = FontStyle.Bold
		};

		GUILayout.BeginVertical();
		GUILayout.Label(entry.Description.Description, styleNormal, GUILayout.ExpandWidth(true));
		GUILayout.Label("Plugin has been disabled!", styleError, GUILayout.ExpandWidth(true));
		GUILayout.EndVertical();
	}
}
