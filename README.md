# Mod Template

A template project for creating RoR2 mods.

# Develop

There are several parameters that you will need to rename in order to turn this repo into a new mod, they are as follows:
1. ModTemplate.sln filename
2. ModTemplate.sln line 6 - Rename "ModTemplate" and "Template.csproj"
3. The "ModTemplate" folder
4. ModTemplate\Template.csproj filename, to match the edit made in the .sln file
5. Template.csproj line 20 - change the DeployFolder to the location that your mod is to be output to
6. Template.csproj line 25 - change the .dll name to the name of your mod
7. The title on line 1 of this README
8. ModTemplate\src\Log.cs line 3 - rename the namespace
9. ModTemplate\src\Main.cs line 4 - rename the namespace to match the above step
10. ModTemplate\src\Main.cs line 12 - rename to the name of your mod
11. If you want to make sure that your mod project is clean then you can ctrl+F "Template" and delete all the files that the build task automatically generates

# Deploy

1. Update `version_number` in `manifest.json`
2. Update `PluginVersion` in `Main.cs`
3. Update the changelog in this readme
4. Execute `$ dotnet build --configuration Release`
5. Bundled package is located in `/dist`

# Changelog

**0.0.1**

-   Mod template
