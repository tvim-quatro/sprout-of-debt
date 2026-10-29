# Sprout of Debt

A cozy 2D pixel-art game made in Unity: grow plants in the **Garden**, brew potions in the
**Laboratory**, sell them to customers at the **Counter**, and pay off your debt to the
**Moneylender**.

## Getting started

1. Install **Unity 6000.6.3f1** via Unity Hub (no extra platform modules needed to work in the editor).
2. Hub > Projects > Add > *Add project from disk* > select this folder (the one with `Assets/` and `ProjectSettings/`).
3. Open the project (the first import takes a few minutes), then open `Assets/Scenes/MainMenu.unity` and press Play.
4. Set Edit > Settings > External Tools > External Script Editor to your IDE (VS Code, Rider or Visual Studio).
5. Register Unity Smart Merge for scenes/prefabs once (see [Smart Merge](#smart-merge)).

## Tech

Unity 6 · URP · uGUI + TextMeshPro · Input System · C#

## Project structure

```
Assets/
  Art/          game sprites: Backgrounds, Characters, Items, Garden, Laboratory, Effects, Tutorial
  UI/           interface: Art (sprites from Figma), Fonts, Prefabs, Editor (prefab generators)
  Data/         ScriptableObject data (ingredients, potions, recipes, ...)
  Scenes/       MainMenu, gameplay scenes, Sandbox/ test scenes
  Scripts/      C# code, one folder per game area
  Settings/     render pipeline settings
  TextMesh Pro/ Unity-managed TMP resources
```

Full rules (folders, naming, code style, git) are in [AGENTS.md](AGENTS.md).

## Workflow

- Branch from `main`: `feat/...`, `fix/...`, `chore/...`. Never push to `main` directly.
- Open a PR, describe how to test it in the editor, and get a review from a teammate.
- Don't commit changes Unity makes to `ProjectSettings/` or `Packages/` unless they're intended.

## Smart Merge

Lets git merge `.unity` / `.prefab` files with Unity's YAML merge tool. Run once:

```bash
# macOS
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver '"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/Tools/UnityYAMLMerge" merge -p %O %B %A %A'
git config --global merge.unityyamlmerge.recursive binary
```

```bash
# Windows
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'
git config --global merge.unityyamlmerge.recursive binary
```

## Links

- Game design docs: [Google Drive](https://drive.google.com/drive/u/0/folders/1_fW2kvgHUg0a6tA0w-hMHTSlmFFxm7QE)
- UI design: [Figma, SOD-Design](https://www.figma.com/design/PJYjcGJSAvnbuQy0wUDh74/SOD-Design?node-id=90-140)
