# Example assets

ItemManager loads items from an AssetBundle embedded in the mod DLL.
To try the item example, build a bundle named `hs_example_bundle` in Unity with a prefab `HS_Example_Hat`,
put the bundle file in this folder, and embed it in the .csproj:

```xml
<EmbeddedResource Include="Examples\assets\hs_example_bundle" Condition="'$(IncludeExamples)' == 'true'" />
```

Bundles are not committed to this template.
