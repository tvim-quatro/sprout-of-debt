# AGENTS.md

Shared rules for everyone working in this repo: humans and AI assistants (Claude, Gemini,
Copilot, Cursor, ...). `CLAUDE.md` and `GEMINI.md` only import this file, so edit the rules
**here**. When a rule conflicts with what you'd do by default, the rule wins. If something is
missing, follow the patterns already in the code and propose an update to this file in your PR.

## Project

**Sprout of Debt** is a
2D pixel-art game made in Unity: grow plants in the **Garden**, brew potions in the
**Laboratory**, sell them at the **Counter** to customers, and pay off the **Moneylender**.
An **Imp** acts as the tutorial/helper character.

Source of truth for *what* to build (read these before implementing a feature; if the task
description in chat is missing context, check here instead of guessing):

- **Game design doc and other docs:** [Google Drive](https://drive.google.com/drive/u/0/folders/1_fW2kvgHUg0a6tA0w-hMHTSlmFFxm7QE)
- **UI / visual design:** [Figma, SOD-Design](https://www.figma.com/design/PJYjcGJSAvnbuQy0wUDh74/SOD-Design?node-id=90-140). UI art in `Assets/UI/Art` is exported from it.
- **Repository:** https://github.com/tvim-quatro/sprout-of-debt

If code and the GDD disagree, follow the GDD and point out the mismatch to the user.

## Tech stack

- **Unity 6000.6.3f1** (see `ProjectSettings/ProjectVersion.txt`). Everyone uses exactly this
  version; decline upgrade prompts. Editor/package upgrades are a separate, agreed PR.
- **URP** (Universal Render Pipeline) with the **2D Renderer** (`Assets/Settings/Renderer2D.asset`),
  used by both `PC_RPAsset` and `Mobile_RPAsset`. Don't add 3D renderers.
- **uGUI + TextMeshPro** for UI (Canvas, `Image`, `Button`, `TMP_Text`). Not UI Toolkit.
  Canvas reference resolution is 1920x1080, `ScaleWithScreenSize`, match 0.5.
- **New Input System only** (`activeInputHandler: 2`). Use `UnityEngine.InputSystem` and
  `InputSystemUIInputModule`; never `UnityEngine.Input.GetKey` / legacy `StandaloneInputModule`.
  Actions live in `Assets/InputSystem_Actions.inputactions`.
- Font: **Pixelify Sans** (TMP asset `Assets/UI/Fonts/PixelifySans_FontAsset.asset`).
  Add a new font only together with its TMP font asset and a place that uses it.

## Folder layout

Game content lives directly under `Assets/`, grouped by **asset type**, then by **game area**.
Area names are the same everywhere: `Garden`, `Laboratory`, `Counter`, `Characters`, `Items`,
`Economy`, `Tutorial`, `UI`, `Core`. This is the existing layout; keep it.

```
Assets/
  Art/                    Game-world sprites (not UI)
    Backgrounds/          area backgrounds (garden_bg, lab_bg, counter_*)
    Characters/           Customers/ Imp/ Moneylender/   (one PNG per emotion)
    Items/                Ingredients/ Potions/ Seeds/   (item icons)
    Garden/ Laboratory/   area props, plant stages, frame sequences
    Effects/              fx_* sprites
    Tutorial/             tutorial overlays
  UI/                     Everything for the interface (uGUI)
    Art/                  UI sprites from Figma: Buttons/ Panels/ Bars/ Controls/ Icons/ Cursors/ Backgrounds/
    Fonts/                TTF + TMP font assets
    Prefabs/              Buttons/ Panels/ Controls/ Bars/ Canvases/
    Editor/               editor tools that generate UI prefabs / scenes
  Data/                   ScriptableObject assets: Ingredients/ Potions/ Recipes/ Seeds/ Customers/ ...
  Prefabs/                game (non-UI) prefabs by area: Garden/ Laboratory/ Counter/ Characters/
  Animations/             <Area>/ Animation Clips and Animator Controllers
  Audio/                  Music/ SFX/<Area>/
  Scenes/                 MainMenu.unity (build index 0), gameplay scenes; Sandbox/<Area>_Sandbox.unity
  Scripts/                <Area>/ C# code (Core/ Items/ Laboratory/ Garden/ Counter/ Economy/ UI/ ...)
  Settings/               URP assets, volume profiles
  Tests/                  EditMode/ PlayMode/
  ThirdParty/             <Vendor>/ imported Asset Store / external assets
  TextMesh Pro/           TMP package resources (Unity-managed, do not edit)
  InputSystem_Actions.inputactions
```

Rules:
- Put a file in the existing `Assets/<Type>/<Area>/` folder. If none fits, ask before
  inventing a folder. Never create new top-level folders in `Assets/` without team agreement.
- Don't create empty folders "for later": create a folder together with its first asset.
- Game art goes in `Art/`; UI art goes in `UI/Art/`. Don't mix them.
- Editor-only code goes in an `Editor/` folder (`UI/Editor/` for UI tools,
  `Scripts/<Area>/Editor/` otherwise).
- Use the full area name `Laboratory`, not `Lab`, for folders, namespaces and classes.
- ScriptableObject assets: `Data/<Type>/<Type>_<Name>.asset`
  (`Ingredient_Lavender`, `Potion_EmberBrew`, `Recipe_ShadowTonic`).
- Test scenes for one area: `Scenes/Sandbox/<Area>_Sandbox.unity`, not in Build Settings.
- Imported third-party assets go to `Assets/ThirdParty/<Vendor>/`, never mixed into our folders.
- No duplicate or near-duplicate folders (`Art` vs `Sprites`, `Script` vs `Scripts`,
  `Lab` vs `Laboratory`). No `.bak`, test, sample or template leftovers in the repo.
- When an import tool (Figma importer, Asset Store package) generates files, keep only what
  the game uses, put it in the right folder and delete the rest in the same PR.

## Code structure and modularity

- One class = one responsibility. Split a class when it mixes concerns (UI + game state,
  input + logic, data + behaviour). Soft limit for **code we write by hand** (all `.cs`, including
  editor tools): **~300 lines per file**, ~40 lines per method. Files Unity generates or
  serializes (`.unity`, `.prefab`, `.asset`, `.meta`, `ProjectSettings`) are exempt.
  If a file grows past that, split it (see how `UIPrefabBuilder` delegates to
  `ButtonPrefabBuilder`, `PanelPrefabBuilder`, ... sharing `UIPrefabUtils`).
- **Data in ScriptableObjects, behaviour in MonoBehaviours.** Static game content (plants,
  seeds, ingredients, potions, recipes, customers, prices, debt milestones) is defined in
  `ScriptableObject` classes named `<Thing>Definition` (`IngredientDefinition`,
  `PotionDefinition`, `RecipeDefinition`), created via
  `[CreateAssetMenu(fileName = "<Thing>_", menuName = "Sprout of Debt/<Area>/<Thing>")]`, with
  assets in `Data/<Type>/`. Definitions are read-only at runtime: changing state (stock,
  growth progress, current brew) lives in runtime classes, never in the asset.
  Never hardcode game-balance numbers or item lists in code.
- Definitions reference each other by asset reference, not by string name. Each definition
  that is saved or looked up has a stable `id` string field.
- Debug/cheat helpers live in `Scripts/<Feature>/Debug/` and are wrapped in
  `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. No temporary/placeholder classes in `main`
  (no `Temp*`, `Test*`, `New*` types); build the real system or leave a TODO in an issue.
- **UI only displays and forwards input.** UI scripts (`*View`, `*Controller` in `Scripts/UI/`)
  read state and call into gameplay systems; they don't own game rules. Gameplay code must not
  reference UI types.
- Systems talk via C# events / `UnityEvent` or a small event channel, not by searching for
  each other. Avoid static singletons except for one explicit bootstrap/service locator in `Scripts/Core/`.
- Each game area (Garden, Lab, Counter, Economy, Tutorial, Save) is its own folder and namespace
  and should not depend on another area's internals. Shared types go in `Scripts/Core/`.
- Every feature folder in `Scripts/` has its own Assembly Definition from the start
  (`SproutOfDebt.Garden.asmdef`, references only `SproutOfDebt.Core` and what it truly needs).
  This keeps dependencies explicit, blocks circular references and shortens compile times.
  Editor code gets its own `SproutOfDebt.<Feature>.Editor.asmdef` limited to the Editor platform.
- Save data: one serializable save model in `Scripts/Save/`; don't scatter `PlayerPrefs` keys
  around the codebase (the current `"HasSavedGame"` check in `MainMenuController` is a placeholder).

## Unity 2D best practices

- Use **sprites in world space** for game scenes (Garden, Lab, Counter) with an orthographic
  camera; use the uGUI Canvas only for HUD, menus and popups.
- Render order: use **Sorting Layers** (`Background`, `Default`, `World`, `Characters`, `FX`,
  `UI`, back to front) and `Order in Layer`, not the Z position.
- Pixel-art import settings for every sprite: Filter Mode **Point**, Compression **None**,
  Generate Mip Maps **off**, consistent Pixels Per Unit. 9-slice sprites (`_9s`) must have
  borders set in the Sprite Editor. Use a **Sprite Atlas** per area (`UI`, `Garden`,
  `Laboratory`, ...) next to the art they pack.
- Sprites are exported from Figma at **3x** (1 art pixel = 3x3 texture pixels). Every new
  sprite must be exported at the same 3x scale and imported with Pixels Per Unit **100**, so
  all art has the same pixel size on screen. Never mix scales.
- Use the URP **2D Renderer** (2D lights if needed), no 3D lights / SSAO / 3D post-processing,
  and the Pixel Perfect Camera component for pixel-crisp output.
- Animation: frame sequences (`_01..04`) become Animation Clips via the Animator or a simple
  sprite-swap component; clips and controllers go in `Animations/<Area>/`.
- Prefer prefabs for anything placed more than once (plants, beds, customers, items) and
  prefab variants for small differences. Keep scenes thin: a scene mostly instantiates prefabs.
- Cache component references (`Awake`), avoid per-frame allocations, `Find*`, and `GetComponent` in `Update`.
- Use `CompareTag` instead of `tag ==`; use `TryGetComponent`.
- Don't use `Resources/` for game content; use serialized references (or Addressables later, if the team decides).

## C# conventions

Match `Assets/Scripts/UI/MainMenuController.cs`:

- Namespaces: `SproutOfDebt.<Feature>` for runtime code (`SproutOfDebt.UI`, `SproutOfDebt.Garden`),
  `SproutOfDebt.<Feature>.Editor` for editor code. Namespace mirrors the folder.
- One public type per file; file name = type name. PascalCase types, methods and properties.
- Private fields: `camelCase`, no `m_`/`_` prefix. **No public fields**, including on
  ScriptableObjects: expose to the Inspector with `[SerializeField] private` and give other code
  read-only access through a property (`public int Price => price;`). Mutate state only via methods.
- Explicit access modifiers on every member; `private` by default, `public` only what other
  systems need. Group them with `[Header("...")]` and add a
  `[Tooltip]` when the meaning isn't obvious.
- Constants: `private const string PascalCase = ...` (see `UIPrefabBuilder`).
- Allman braces, 4-space indentation.
- UI button handlers are public methods named `On<Thing>Clicked()`, wired via the Button's
  `onClick` (persistent listeners) rather than `GetComponent` lookups in `Update`.
- Null-check optional Inspector references before use; don't let a missing reference throw.
- Log with a tag prefix: `Debug.Log("[MainMenu] ...")`. Remove noisy logs before merging.
- Guard editor-only API with `#if UNITY_EDITOR` in runtime scripts.
- Keep logic out of `Update` when an event or callback works.
- Avoid `GameObject.Find` / `FindObjectOfType` in gameplay code; use serialized references.
- Comments in English. Reference the GDD when implementing a design rule
  (e.g. `// According to GDD: ...`).

## Assets and naming

Asset files use `snake_case` with a category prefix:

| Kind | Pattern | Example |
| --- | --- | --- |
| UI sprite | `ui_<element>_<variant>_<state>.png` | `ui_btn_primary_hover.png` |
| 9-slice UI sprite | `..._9s.png` (set sprite borders in import settings) | `ui_panel_window_9s.png` |
| UI icon | `ui_icon_<name>.png` | `ui_icon_coin.png` |
| Character | `char_<who>_<emotion>.png` | `char_customer_fox_happy.png` |
| Item icon | `icon_<type>_<name>.png` (`potion`, `seed`, `ingr`) | `icon_seed_lavender.png` |
| Environment | `<area>_<thing>[_state]` (`garden_`, `lab_`, `counter_`; `_stage2`, `_ripe`, `_01..04` for frames) | `garden_plant_mandrake_ripe.png` |
| Audio | `music_<name>`, `sfx_<area>_<action>` | `sfx_lab_bubble.wav` |
| Effects | `fx_<name>[_NN].png` | `fx_coin_03.png` |

- Prefabs use `PascalCase` with a type prefix: `Button_Primary`, `Panel_Window`,
  `Control_Slider`, `Control_DebtBar`, `SOD_UI_Canvas`.
- Scenes: `PascalCase.unity`. Add new scenes to Build Settings.
- Plant/item names are shared across Seeds, Ingredients, Potions and Garden sprites
  (`lavender`, `mandrake`, `moonpetal`, `nightshade`, `emberroot`). Reuse the same id string in code.

## UI workflow

- UI prefabs and the MainMenu scene are generated by editor tools in `UI/Editor/`:
  `Tools > UI > Generate All UI Prefabs` (`UIPrefabBuilder`, which delegates to
  `ButtonPrefabBuilder`, `PanelPrefabBuilder`, `ControlPrefabBuilder`, `BarPrefabBuilder`,
  `CanvasPrefabBuilder`; shared paths/helpers in `UIPrefabUtils`) and
  `Tools > UI > Build Main Menu Scene` (`MainMenuSceneBuilder`).
- Editor tools run **only when invoked from a menu**; they must never create or
  overwrite assets automatically on editor load (`[InitializeOnLoad]`), because that creates
  duplicates with new GUIDs on other machines.
- If you change a generated prefab's look or structure, change the builder script and
  regenerate, otherwise the next regeneration overwrites your manual edits. Say in the PR
  if you edited a generated asset by hand.
- Build screens from the existing prefabs in `UI/Prefabs/` instead of new ad-hoc
  buttons/panels. If a new reusable element is needed, add it to `UIPrefabBuilder`.

## Unity files and git

- **Always commit `.meta` files** together with their asset, and never delete or regenerate
  a `.meta` by hand: GUIDs in `.meta` files hold all references between assets.
- Move/rename assets **inside the Unity Editor**, not in the file system, so the `.meta` follows.
- Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `Builds/`, `*.csproj`, `*.sln`
  (already in `.gitignore`). Also never commit `.DS_Store`, `.vscode/`, `.idea/`, `*.slnx`:
  they are personal or regenerated by Unity on every machine.
- Line endings are LF everywhere (enforced by `.gitattributes`). In Unity set
  Project Settings > Editor > Line Endings For New Scripts = **Unix**.
- Scenes and prefabs merge via Unity Smart Merge (`merge=unityyamlmerge` in `.gitattributes`);
  each developer registers the `unityyamlmerge` driver in their git config once.
- Scenes and prefabs are YAML but merge badly. Don't edit the same scene/prefab as someone
  else at the same time; tell the team in chat when you're working on a scene.
  Prefer splitting work into prefabs over growing one big scene.
- An AI assistant must not hand-edit `.unity`, `.prefab`, `.asset` or `.meta` YAML unless the
  change is trivial and well understood; prefer C# (runtime or editor scripts) and let the
  user make the change in the Editor.
- Don't commit package or editor upgrades, `ProjectSettings` changes or renderer changes as a
  side effect of a feature. If Unity modifies them on open, discard those changes unless they're intended.

## Branches, commits, PRs

- Branch from `main`: `feat/<short-name>`, `fix/<short-name>`, `chore/<short-name>`
  (existing example: `feat/ui-kit`).
- Never push directly to `main`; open a PR and get a teammate's review.
- Commit messages in English, imperative mood ("Add garden watering", "Fix debt bar fill").
  PR titles may use the conventional form `feat(ui): ...`, `fix(garden): ...`.
- Keep PRs focused on one feature. Describe what changed, how to test it in the Editor
  (which scene, which menu item), and add a screenshot/GIF for visual changes.
- Before opening a PR: the project opens with no compile errors and no new Console errors,
  and the affected scene runs in Play mode.

## Known deviations (refactor later, by the owners)

The rules above describe how things **should** be done from now on. Some existing content was
created before them. Don't "fix" these as a side effect of another task; they will be handled
in dedicated refactoring PRs by their owners:

- No asmdefs yet; existing scripts compile into `Assembly-CSharp`. New feature folders get
  their asmdef from the start.
- `MainMenuController` hardcodes the scene name and uses a `PlayerPrefs` placeholder (owner is
  working on it; don't change it as part of other tasks).
- `SampleScene` is a template placeholder; it stays until the real gameplay scene replaces it,
  because Main Menu's New Game/Continue load it.

## For AI assistants specifically

- Read the relevant existing scripts before adding new ones; extend instead of duplicating.
- Don't add new packages, frameworks (DI containers, DOTween, etc.) or architecture layers
  without the user asking for it.
- Don't rename or move existing assets, namespaces or scenes as a side effect.
- Don't introduce a different UI system (UI Toolkit / UXML) or the legacy Input Manager.
- You cannot run the Unity Editor: say so, and tell the user what to check in the Editor
  (scene, menu item, Inspector fields to assign) after your change.
- Keep changes minimal and scoped to the task; leave unrelated code as is.
