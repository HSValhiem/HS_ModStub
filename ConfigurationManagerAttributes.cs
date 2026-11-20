using System;
using BepInEx.Configuration;

// Optional display hints read by BepInEx ConfigurationManager (F1 menu) through reflection.
// Only the field names matter; add more fields from the ConfigurationManager documentation when you need them.
#pragma warning disable CS0649
internal sealed class ConfigurationManagerAttributes
{
	public bool? Browsable;
	public string? Category;
	public Action<ConfigEntryBase>? CustomDrawer;
	public bool? HideDefaultButton;
	public bool? HideSettingName;
	public int? Order;
	public bool? ReadOnly;
	public bool? IsAdvanced;
}
#pragma warning restore CS0649
