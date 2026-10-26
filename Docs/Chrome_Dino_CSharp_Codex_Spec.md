# Unity Chrome Dinosaur - Complete C# Implementation Brief for Codex

Use this entire document as the implementation brief. Implement the project in the actual repository; do not stop at a plan or a collection of code snippets.

## 1. Objective, scope, and source of truth

Build the C# gameplay systems and Unity integration needed for the enhanced Chrome Dinosaur runner described in the attached homework. Produce readable, maintainable, student-explainable code and a playable implementation when the Unity Editor is available.

The primary scope is C#: gameplay, input, state management, interaction inheritance, animation control, UI behavior, persistence, configuration, and meaningful tests. Scenes, prefabs, input assets, Animator assets, and Inspector wiring are integration requirements because scripts alone do not satisfy the assignment. Preserve existing artwork and scene work. New polished artwork is optional; usable placeholders are acceptable during implementation, but mandatory animations and UI must still be demonstrable.

Source documents:

- `HM1.pdf`: four physical PDF pages. Physical page 2 / printed page 1 contains the course rules. Physical pages 3-4 / printed pages 2-3 contain the gameplay and submission requirements.
- `Unsaved Document 2`: the supplied Unity `.gitignore`, reproduced verbatim in Appendix A so this brief can be used independently.

Requirement labels:

- **HW**: explicitly mandatory in the assignment.
- **RULE**: mandatory engineering or delivery rule in this brief.
- **DEFAULT**: a concrete design decision supplied here to resolve something the assignment does not define. Implement it unless existing approved project behavior specifies otherwise, and document any justified replacement.
- **OPTIONAL**: additional polish; implement only after required behavior works.

Follow the user's later instructions and applicable repository instructions. Preserve every HW requirement. If an instruction would remove an HW requirement, flag the conflict before changing that requirement. Ordinary implementation decisions do not need repeated approval.

Do not invent an existing Unity version, package version, student ID, repository URL, build target, test result, or completed Editor configuration. Record assumptions and distinguish implemented, tested, and manually pending work.

## 2. Mandatory assignment coverage

| ID | Required behavior | Implementation evidence |
| --- | --- | --- |
| HW-01 | Continuously moving background using parallax scrolling | At least two visible layers with different movement factors; seamless wrapping during a run |
| HW-02 | Every player input uses Unity's New Input System exclusively | Input Action asset, action-based gameplay controls, and New Input System UI event module; no legacy input implementation |
| HW-03 | Player starts with exactly three lives, shown with a health bar | Health model starts at 3; Canvas bar visibly represents 3, 2, 1, and 0 |
| HW-04 | Space jumps; Shift performs a dash | Working action bindings and observable jump/dash behavior |
| HW-05 | Obstacle contact removes one life; zero lives ends the game | Ground and aerial hazards damage correctly; death occurs once at zero |
| HW-06 | At least one ground obstacle, one aerial obstacle, and one score item | Distinct spawnable prefabs and concrete classes for each |
| HW-07 | All those obstacles/items inherit from the shared base class named `Interactable` | Real inheritance and polymorphic interaction dispatch, not only an unrelated interface or duplicated handlers |
| HW-08 | Animator states `Run`, `Jump`, `Hit`, and `Death`, with correct transitions | Assigned Animator Controller, valid clips, matching script parameters, and tested transitions |
| HW-09 | Brief visual change when hit | Visible temporary sprite color change or animation feedback; implement a brief color flash by default |
| HW-10 | Separate Main Menu and Pause Menu; Canvas always shows live current score and high score during play | Separate panels/screens, working buttons, and synchronized HUD |
| HW-11 | High score stored with `PlayerPrefs` and retained after reopening the game | PlayerPrefs-backed store and verified persistence between launches |

Creative graphics, high-quality animation, and special effects are assignment bonus work. Audio, difficulty scaling, achievements, online scores, crouching, double jump, and gamepad support are not stated homework requirements. Do not let them delay mandatory coverage.

## 3. Default gameplay decisions

These decisions make the implementation concrete; they are not additional claims about the homework text.

| Setting | DEFAULT |
| --- | --- |
| Game format | 2D endless runner, orthographic camera, flat continuous ground |
| Movement convention | Player keeps a fixed horizontal camera anchor; obstacles and scenery move left to simulate forward running |
| Starting / maximum lives | 3 / 3; not changeable to another starting value in the final assignment build |
| Jump | Single grounded jump; pressing Space while airborne does not add another jump |
| Base effective forward speed | 6 world units/second, tuned to the chosen sprite scale |
| Initial jump velocity / gravity scale | 10 world units/second / 3 as starting tuning values; verify using the actual Physics2D gravity |
| Dash | 0.20 seconds at 2x effective forward speed; the world/parallax reflect the speed change and player feedback makes the burst visible |
| Dash cooldown | 1.5 seconds from the end of a dash; one press activates once, holding Shift does not repeat |
| Dash eligibility | Allowed grounded or airborne while Playing, except during nonfatal Hit recovery |
| Dash damage behavior | No invulnerability, obstacle destruction, or bonus lives granted by dash |
| Accepted damage | Exactly 1 life per hazard encounter |
| Damage invulnerability | 1.0 second after an accepted nonfatal hit, using gameplay time |
| Hit animation / flash | Approximately 0.25 seconds / 0.15 seconds; serialized tuning values |
| Coin value | 10 points, collected once |
| Distance score | `floor(totalForwardDistance * pointsPerWorldUnit)`, with initial rate 1 point/world unit |
| Total current score | Distance points + collected item points |
| Initial spawn interval | Random interval in the 1.2-2.0 second range, constrained further by safe spacing |
| First hazard | At least 2.0 seconds after starting, with enough visible warning before contact |
| Pause | Escape toggles Playing/Paused; a visible Pause button also opens the menu |
| Quit | Main Menu Quit exits a standalone player; Editor behavior is documented |
| UI language | English labels unless the existing project uses another language |

Expose balance values through one small `RunnerSettings` ScriptableObject, or preserve an equivalent existing configuration arrangement. Use clear units in field names/tooltips. Validate positive durations/speeds, sensible intervals, and nonnegative points. No configurable values may undermine HW requirements in the final scene.

Do not add crouch-dependent aerial hazards. Every generated hazard sequence must be survivable using the available jump/dash controls or by remaining grounded where appropriate. Tune defaults in Play Mode; numerical defaults are a starting point, not proof of fair gameplay.

## 4. Repository inspection and environment rules

Before editing:

1. Read applicable `AGENTS.md` and project documentation.
2. Inspect `git status` and preserve all unrelated user changes. Do not reset, clean, rebase, or replace the repository to make the task easier.
3. Locate the Unity project root. It normally contains `Assets/`, `Packages/`, and `ProjectSettings/`.
4. Read `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `Packages/packages-lock.json`, input settings, assembly definitions, and relevant scripts/scenes/prefabs.
5. Identify the render pipeline, current input implementation, UI text components, build scenes, and existing tests.
6. Identify whether the required Unity Editor version and build modules are available.

**RULE:** Use the project's recorded Editor version and installed compatible packages. Do not migrate Unity, change the render pipeline, or recreate the project just to use a preferred API. Match that Editor's C# language and .NET/API compatibility profile; do not assume a modern standalone .NET SDK defines what Unity supports.

If no project exists, create a conventional 2D Unity project using an available supported Editor version and record the exact version. If no Editor is available, produce the full C# implementation and valid text-based assets that can be safely authored, with explicit compatibility assumptions and a complete setup checklist. Do not pretend to have created/imported scenes, Animator controllers, `.meta` files, lockfiles, or builds that were never validated by Unity.

Check installed-version documentation for version-sensitive APIs. For example, Unity 6 uses `Rigidbody2D.linearVelocity`; earlier supported project versions may use a different property. Target the actual version. Do not introduce reflection or broad compatibility wrappers unless multiple Editor versions are an explicit project requirement.

## 5. Package and dependency management

**RULE:** Unity Package Manager (UPM) is the dependency mechanism for this project. Do not add npm, pnpm, pip, NuGetForUnity, a standalone .NET solution, or downloaded DLLs to implement ordinary runner gameplay.

| Dependency | Policy |
| --- | --- |
| `com.unity.inputsystem` | Required; reuse the installed compatible version or add a released version compatible with the actual Editor |
| Unity UI / `com.unity.ugui` | Use for Canvas, health bar, panels, and buttons; preserve the compatible project setup |
| TextMeshPro / `TMPro` | Prefer existing TMP components; package ownership varies with Unity version, so inspect the project before adding a separate package |
| `com.unity.test-framework` | Use a compatible version for Edit Mode and Play Mode tests; reuse existing installation |
| Existing 2D/render pipeline packages | Preserve unless a documented requirement demands a targeted change |
| DOTween, UniTask, Zenject, VContainer, Odin, Addressables, networking packages | Do not add for this assignment; built-in facilities are sufficient |

Package rules:

1. Pin direct registry dependencies to exact released versions in `Packages/manifest.json`. No speculative version numbers, floating Git branches, machine-specific absolute paths, or preview packages unless the existing project requires one.
2. Keep `Packages/manifest.json` and Unity-generated `Packages/packages-lock.json` in version control. Let Unity resolve/update the lockfile; do not fabricate dependency entries or delete it as a routine fix.
3. Do not update unrelated dependencies or remove template packages during this task merely for tidiness.
4. Use official Unity packages first. Any exceptional third-party dependency needs a concrete reason, source, license, compatibility note, and pinned version/commit.
5. Do not modify `Library/PackageCache`, global package caches, or generated `.csproj` files. Make project changes in tracked configuration/assets.
6. If assembly definitions are present or useful for separating tests, reference actual installed assemblies, such as `Unity.InputSystem`, the installed UI/TMP assemblies, and the runtime assembly. Verify their names rather than guessing.
7. Keep Editor-only and test-only dependencies out of runtime/player assemblies. Editor scripts live under an `Editor/` folder or an Editor-only assembly definition.
8. Add a short dependency table to `README.md`: exact Editor version, direct package IDs/versions, purpose, and any setup/import step. A fresh checkout should resolve dependencies through UPM.
9. A package change is complete only after dependency resolution and compilation are checked in Unity, or it is clearly marked unverified because Unity is unavailable.

## 6. Clean C# and Unity coding rules

### 6.1 Readability and structure

- Use English identifiers and concise English comments unless repository conventions require another language.
- Use PascalCase for types, methods, public properties/events, and enum members. Use camelCase locals/parameters and `_camelCase` private fields unless the repository has a consistent different convention.
- One main attachable MonoBehaviour/ScriptableObject per file; filename matches its type. Organize by feature and use a consistent namespace, initially `ChromeDino` with small subnamespaces if useful.
- Use four-space indentation, braces consistently, and an `.editorconfig` compatible with the existing tooling. Do not reformat unrelated files.
- Prefer short cohesive methods, early returns, clear state names, and named configuration/constants over nested conditionals and unexplained numbers.
- Comments explain intent, constraints, or Unity-specific pitfalls. Do not narrate obvious statements or add decorative banners everywhere.
- Use composition for movement, feedback, UI, and services. Use inheritance specifically where the homework requires `Interactable` and where it removes genuine duplication.
- Avoid giant manager classes, deep inheritance, service locators, reflection-based wiring, generic frameworks, and one interface per class. Introduce an interface only at a useful boundary, such as a persistence store used in tests.
- Separate gameplay decisions from input interpretation and presentation. UI must not own lives/score; animation must not decide whether damage is valid.
- Prefer named events/typed C# events for model notifications. Use UnityEvents where they are useful for Inspector button wiring. Avoid string-based `SendMessage` dispatch.
- Do not leave required code as `TODO`, empty event handlers, `NotImplementedException`, or pseudo-code. Document genuinely unavailable art/setup separately.

### 6.2 Unity lifecycle and references

- Use private `[SerializeField]` fields for assignable configuration and dependencies; expose read-only properties when consumers need data. Do not make mutable fields public for convenience.
- Use `Awake` for local component caching/validation; use explicit initialization or a documented bootstrap step for cross-object dependencies. Avoid relying on unspecified execution order between different objects' `Awake` methods.
- Match event/action subscriptions with unsubscriptions in `OnEnable`/`OnDisable`, or an equally clear lifetime pair. Re-enabling a component must not duplicate callbacks.
- Dispose only resources/actions the component owns. Do not destroy shared Input Action assets or disable shared UI actions because a gameplay listener was disabled.
- Cache component lookups. Avoid `Find*`, `GameObject.Find`, `GetComponent`, LINQ, string formatting, and managed collection allocations inside hot update loops where practical.
- Use serialized references for scene dependencies. A one-time validated lookup for a component on the same object is acceptable. Do not use scene-wide searches as dependency injection.
- Validate required references once with helpful object/component names. Invalid setup should produce a clear error and disable the affected behavior or fail startup safely, rather than produce a null-reference error every frame.
- Use `[RequireComponent]`, `[DisallowMultipleComponent]`, `[Min]`, tooltips, and `OnValidate` where they help. Do not perform asset writes, play-state changes, or complex scene mutations inside `OnValidate`.
- Use `Update` for presentation/frame work and `FixedUpdate` for Rigidbody2D motion and queued physics commands. Apply the appropriate delta time exactly once.
- Use Physics2D consistently; do not mix 3D colliders/rigidbodies with 2D callbacks.
- Use Animator parameter hashes cached with `Animator.StringToHash`. Do not scatter parameter-name strings across scripts.
- Maintain a single owner of game state and `Time.timeScale`. Do not let unrelated components pause/resume independently.
- No per-frame `PlayerPrefs.Save`, permanent debug spam, silent catch-all exception handling, or production cheats that bypass grading behavior.

### 6.3 Practical performance and reset safety

- Bound the number of active obstacles/items. Despawn anything that has passed the left cleanup boundary.
- Instantiate/destroy is acceptable at this assignment's modest spawn rate. Add a small prefab-specific pool only if already present or justified; do not build a generic pooling framework.
- If pooling is used, reset collection/contact flags, collider state, position, velocity, sprite tint, timers, trails, and event registrations before reuse.
- Restore temporary effects deterministically on expiry, death, reset, or disable. Capture and restore the original sprite color instead of assuming it was white.
- Avoid static mutable run state. Reset any unavoidable static state when entering Play Mode, including when domain reload is disabled.
- Treat missing artwork and missing mandatory animation wiring as visible integration tasks; do not silently remove the feature to keep the Console clean.

## 7. Suggested architecture and file responsibilities

Use the existing project organization when it already provides these boundaries. The names below are a suggested minimal layout, not a demand to create unused classes. `Interactable` is the one class name explicitly required by the homework.

| Area / suggested file | Responsibility |
| --- | --- |
| `Core/GameState.cs` | Enum: MainMenu, Playing, Paused, GameOver |
| `Core/GameSession.cs` | Own state transitions, run start/reset/end, pause, and coordinated initialization |
| `Configuration/RunnerSettings.cs` | Serialized balance and world settings with validation |
| `Input/RunnerInputReader.cs` | Own gameplay/system action subscriptions and expose jump/dash/pause requests |
| `Player/PlayerController.cs` | Ground checks, queued jump commands, dash state, and player-side interaction dispatch |
| `Player/PlayerHealth.cs` | Lives, invulnerability, accepted damage, one-time death notification |
| `Player/PlayerAnimationController.cs` | Translate gameplay/health state into Animator parameters |
| `Player/PlayerHitFeedback.cs` | Temporary sprite flash and cleanup; dash feedback can remain in the player visual component |
| `World/WorldScrollController.cs` | Single effective speed/distance source shared by moving world and scoring |
| `World/WorldMover.cs` | Move a hazard/item left using the shared speed and despawn boundary |
| `World/ParallaxLayer.cs` | Wrap repeated background tiles at a layer-specific speed factor |
| `World/SpawnController.cs` | Spawn valid ground/aerial hazards and coins using safe intervals/spacing |
| `Interactables/Interactable.cs` | Abstract common base, interaction guards, and polymorphic interaction entry point |
| `Interactables/Obstacle.cs` | Optional common hazard subclass; consolidate damage behavior if used |
| `Interactables/GroundObstacle.cs` | Concrete ground hazard |
| `Interactables/AerialObstacle.cs` | Concrete aerial hazard |
| `Interactables/CoinCollectible.cs` | Concrete score item, collection exactly once |
| `Scoring/ScoreTracker.cs` | Current distance/item score and live high score |
| `Persistence/PlayerPrefsHighScoreStore.cs` | Read/write/flush only the game's high-score preference |
| `UI/HudController.cs` | Show score/high score/health from model state |
| `UI/MenuController.cs` | Main/Pause/GameOver panel visibility and button forwarding |

Suggested directories under the Unity project root:

```text
Assets/_Project/Scripts/{Core,Configuration,Input,Player,World,Interactables,Scoring,Persistence,UI}
Assets/_Project/Input/RunnerControls.inputactions
Assets/_Project/Settings/
Assets/_Project/Prefabs/
Assets/_Project/Scenes/
Assets/_Project/Animations/
Assets/_Project/Tests/{EditMode,PlayMode}
Assets/_Project/Editor/                  # only if an Editor helper is useful
Docs/
README.md
.editorconfig
.gitignore
```

Keep the architecture small. A scoped `GameSession` composition root with explicit references is sufficient. Do not introduce a persistent singleton/bootstrap scene unless the existing multi-scene project requires it. A single gameplay scene containing separate menu panels is a valid default.

## 8. Game state, transitions, and reset contract

| Current state | Trigger | Result |
| --- | --- | --- |
| MainMenu | Play button | Reset and start a fresh Playing run |
| Playing | Pause action / Pause button | Paused; preserve exact run state |
| Paused | Resume button / Pause action | Playing; continue preserved run |
| Playing | Health reaches zero | GameOver exactly once |
| Paused or GameOver | Restart button | Reset and start a fresh Playing run |
| Paused or GameOver | Main Menu button | Flush record, clean up the run, show MainMenu |
| MainMenu | Quit button | Flush pending record and quit standalone player |

Invalid/repeated transitions are harmless. Escape in MainMenu/GameOver does not start or restart the run. After GameOver, queued input/contact events cannot modify score, lives, or state.

Pause contract:

- `Time.timeScale = 0` while Paused; gameplay commands, world motion, Physics2D, scoring, spawn countdowns, dash/cooldown, invulnerability, hit feedback, and gameplay animation all stop.
- Menu input remains active so Resume works at timeScale zero. Menu-only visual transitions, if any, use unscaled time.
- Resume restores normal timeScale without changing the remaining dash or invulnerability durations. Clear queued gameplay presses rather than executing buttons pressed while paused.
- Keep the physics fixed timestep unchanged for this binary pause/resume implementation.

GameOver contract:

- Freeze gameplay through state gates and disable/freeze player physics as needed, while keeping `Time.timeScale = 1` so the Death animation can finish.
- Cancel dash and nonfatal hit recovery; Death takes priority over Hit.
- Stop spawning, distance gain, item collection, and damage immediately. Final score remains visible and a GameOver panel supplies Restart/Main Menu buttons.
- Restore all temporary visuals and player physics settings when the next run begins.

Run initialization/reset sequence must be explicit and work repeatedly:

1. Disable gameplay commands and stop the prior run.
2. Restore timeScale to 1; clear queued input and previous timers.
3. Remove/reset all prior obstacles/items and reset spawn/parallax positions.
4. Restore player start position, Rigidbody2D settings, zero velocity, grounded state, sprite tint, Animator state/parameters, health=3, dash/cooldown, and invulnerability.
5. Set current distance and score to zero while retaining high score.
6. Update HUD/panels immediately, enter Playing, then enable eligible gameplay input and world movement.

Pause, Restart, and Main Menu buttons must function after repeated runs without duplicate event subscriptions or a leftover timeScale of zero. Do not use scene reload as a substitute for understanding state cleanup; if the existing design reloads a scene, document and verify the global time/state reset.

## 9. New Input System specification

**HW/RULE:** Use `UnityEngine.InputSystem` and Input Actions exclusively for user input. Never implement controls with `UnityEngine.Input`, `Input.GetKey*`, `Input.GetButton*`, `Input.GetAxis*`, legacy mouse polling, or IMGUI input handlers. Do not use `StandaloneInputModule`. Set Active Input Handling to **Input System Package (New)** rather than Both.

Default action layout:

| Map | Action | Type / behavior | Binding |
| --- | --- | --- | --- |
| Gameplay | Jump | Button; one request on press | `<Keyboard>/space` |
| Gameplay | Dash | Button; one request on press | `<Keyboard>/leftShift`, optionally rightShift as an additional binding |
| System | Pause | Button; one request on press | `<Keyboard>/escape` |
| UI | Point | Pointer position | Standard mouse bindings |
| UI | Click | Pointer button | Standard mouse primary button |
| UI | Navigate | 2D navigation | Arrow keys; existing UI bindings may be retained |
| UI | Submit | Button | Enter |
| UI | Cancel | Button | Bind consistently with the pause policy or route to the same single handler |

Use the installed Input System's standard UI action layout/types where appropriate. Assign the UI actions to one `EventSystem` with `InputSystemUIInputModule` and keep UI/System input active in all menus.

Use one coherent gameplay action ownership approach: serialized `InputActionReference`s or a correctly generated wrapper around `RunnerControls.inputactions`. Prefer action references if generated code cannot be produced/verified in the current environment. Do not combine multiple `PlayerInput` instances, manual polling, generated callbacks, and duplicate performed-event handlers for the same actions.

Lifecycle and routing:

- Subscribe once to action callbacks, enable the appropriate maps, and unsubscribe on disable/disposal. The gameplay reader must remain able to receive Pause while Paused.
- Do not globally disable the entire shared action asset when pausing; only gameplay Jump/Dash must be suppressed/disabled.
- Route input requests into gameplay methods; execute jump physics in FixedUpdate. A jump request captured between physics frames must not be lost.
- Hold does not repeatedly jump/dash; invalid requests are discarded, not stored indefinitely for a later state.
- Require release/new press when re-enabling gameplay so holding Space/Shift through Resume or Restart does not trigger a new move unexpectedly.
- Process a pause request before gameplay commands for the same frame, or use an equivalent deterministic gate.
- Handle Escape through exactly one menu/state command path; overlapping UI Cancel/System Pause bindings must not pause and immediately resume in one press.
- Avoid binding UI Submit to Space by default, so a pause-menu submission cannot leak into gameplay Jump.
- HUD Pause is a real UI Button handled by the Input System module, not custom legacy click detection.

Document the asset name, action map names, binding configuration, and whether a generated wrapper is used. No missing generated class dependencies in delivered scripts.

## 10. Player movement, health, and feedback

### 10.1 Physics and jumping

- Use a Dynamic Rigidbody2D with a sensible player collider, frozen Z rotation, and no unintended horizontal drift. Preserve the horizontal camera anchor without teleporting a dynamic body each frame.
- Use a Ground layer mask and a small validated feet overlap/cast or contact check. Ground detection must not identify hazards, coins, or the player's own collider as ground.
- On one eligible Space request, set/apply the configured upward jump impulse/velocity while preserving intended other velocity components. No jump when airborne, paused, in a menu, or dead.
- Grounded becomes false immediately when the jump is accepted and remains false during upward departure even if the feet probe still briefly overlaps the floor. Landing restores grounded only through valid ground contact.
- Hit recovery may temporarily reject Jump/Dash, but the existing jump's vertical motion continues naturally. A visual state transition must not teleport the player to the floor.
- Do not add double jump, wall jump, or crouch unless later requested.

### 10.2 Dash

Use the section 3 default: a short burst of increased effective forward speed. With a camera-anchored runner, moving the world faster is the physical representation of the player's forward dash; it must affect hazard approach, parallax, and distance scoring consistently. Provide visible player dash feedback such as a small lean/tint/trail and expose cooldown state for an optional HUD indicator.

- Only one active dash; no stacking. Triggering during cooldown or Hit recovery does nothing.
- Preserve vertical jump physics. No teleport through hazards and no contact immunity.
- Effective speed is derived from base speed and current dash state; do not multiply a stored speed repeatedly and accumulate drift.
- Dash/cooldown use gameplay time and pause exactly. Accepted damage cancels an active dash and begins its normal cooldown; death/reset clears it. An ignored invulnerable contact does not cancel it.
- After 0.20 gameplay seconds, return to base speed and start the cooldown. Clamp timer transitions so a long frame cannot leave the dash permanently active.
- Verify hazard collisions at the maximum supported speed; a fast dash cannot silently bypass damage through tunneling.

### 10.3 Health and encounter guards

`PlayerHealth` is the only authority that changes lives. Expose read-only current/max lives, invulnerability/death state, an accepted-damage operation returning whether damage happened, and health/death notifications.

- Start at exactly three lives and clamp to `[0, 3]`.
- One accepted hazard encounter subtracts exactly one life. A long overlap or multiple player collider callbacks cannot turn one obstacle into several hits.
- For simultaneous hazards, process contacts deterministically; the first accepted hit starts invulnerability so other contacts during that interval do not immediately remove more lives.
- Each hazard remembers the player's encounter once per activation. A hazard touched during invulnerability must not apply delayed damage merely because the invulnerability expires before the overlap ends.
- Nonfatal damage starts Hit/flash and invulnerability. Fatal damage transitions directly to Death/GameOver, with no competing nonfatal animation transition.
- Death is emitted exactly once per run. No further damage at zero, and no negative lives.
- Coins remain collectible while invulnerable or in nonfatal Hit recovery, provided the game is still Playing.
- Do not heal or replenish lives automatically. New-run reset restores three lives.

## 11. Interactable inheritance and collision dispatch

The assignment explicitly requires the shared base class to be named **`Interactable`**. A valid structure is:

```text
Interactable : MonoBehaviour (abstract)
  Obstacle : Interactable (abstract, optional shared damage logic)
    GroundObstacle : Obstacle
    AerialObstacle : Obstacle
  CoinCollectible : Interactable
```

An indirect subclass still inherits from `Interactable`. Direct subclasses for all three concrete types are also valid if duplication remains small.

Base-class contract:

- Provide a common player interaction entry point and a protected abstract/virtual implementation for subtype behavior.
- Guard interactions against invalid player references, inactive/despawned objects, already processed encounters/collections, and non-Playing game state.
- Dispatch polymorphically. The player must not select effects with `switch`/`if` checks on `GroundObstacle`, `AerialObstacle`, or `CoinCollectible` types.
- Resolve the player/root once for collider hierarchies and guard duplicate callbacks; child colliders must still identify the correct player.
- Keep motion/despawning in a composed WorldMover or equivalent focused component, rather than copying it into each subclass.
- Reset all encounter/collection state on reuse if pooling exists.

Use one interaction dispatch route. Default: the player's `OnTriggerEnter2D` resolves an `Interactable` and calls its public entry point; hazard/item scripts do not independently call health/score again for the same contact.

Ground hazards sit on the floor and are jumpable. Aerial hazards use distinct sprites/placement and move through the playable region; at least some aerial placement must require a meaningful choice, while every possible layout remains avoidable without crouching. Coins call the scoring service once, then immediately disable their interaction collider and despawn. Hazards damage through PlayerHealth; they never manipulate the UI directly.

Set hazard/item colliders as triggers by default so collisions do not knock the camera-anchored player horizontally or trap them. Configure the Physics2D collision matrix so Player interacts with Ground/Hazard/Collectible, but ground checks exclude Hazard/Collectible. Ensure required Rigidbody2D involvement and trigger/collision settings actually generate callbacks in the selected Unity version.

Moving hazard rigidbodies should use physics-consistent FixedUpdate movement. Verify fast trigger contacts at dash speed. If needed, use an appropriate collision detection mode and/or a targeted shape sweep into the same guarded interaction entry point; do not assume a collision mode alone guarantees trigger callbacks.

## 12. World scrolling, parallax, and spawning

### 12.1 Shared movement and timing

- Maintain one effective forward speed and one integrated run distance. Obstacles/items, parallax, and distance score consume that shared source.
- World distance integrates speed over gameplay time once. Do not independently estimate distance from frame count or score updates.
- Rigidbody2D hazard/item movement uses FixedUpdate; visual-only parallax may use Update with the same speed and scaled delta time.
- No world movement or new spawns in MainMenu, Paused, or GameOver.
- Player remains at the agreed horizontal anchor; the camera does not follow drifting objects.

### 12.2 Parallax

- At least two visible background layers with different movement factors; example factors are 0.2 and 0.5, with ground visuals near 1.0.
- Use enough repeated tiles to cover the camera at supported aspect ratios. Wrap a tile behind the rightmost tile after it leaves the left edge.
- Compute or configure rendered tile width consistently. Do not use a pixel width as a world-space distance without conversion.
- Preserve overshoot during wrapping; repeated movement and dash changes cannot create visible gaps/seams or accumulating positional drift.
- Reset all layer positions at run restart. Halt motion exactly during pause/game over.
- The solid ground collider may remain stationary; scrolling ground artwork must not destroy stable player grounding.

### 12.3 Spawn behavior and fair layouts

- Spawn ground obstacles, aerial obstacles, and coins off the right edge at explicit ground/air/item anchor heights.
- Use serialized prefab references and camera-relative spawn/despawn boundaries where practical; no hard-coded screen pixels.
- Use a gameplay-time countdown that starts after the initial grace period and stops outside Playing. Avoid an unbounded spawn burst after Resume or a long frame.
- Use deterministic seeded randomness in tests or a developer test option, while default gameplay uses ordinary random variation.
- Treat the default random interval as a proposal, then enforce geometric minimum spacing and player reachability. Do not spawn overlapping blocking hazards or coins inside hazard colliders.
- Size safe spacing for maximum dash speed, obstacle widths, jump flight/recovery time, and the player's visual reaction window. Do not guarantee spacing solely from base-speed intervals.
- Choose a small number of validated layouts/patterns if necessary to keep random sequences fair. Do not generate arbitrary combinations that cannot be survived.
- Despawn objects left of the cleanup boundary. Running for several minutes must not accumulate unlimited objects, callbacks, or inactive pooled objects.
- Difficulty scaling is optional. Keep base speed constant by default; if existing difficulty scaling is retained, cap it and revalidate all spacing/dash assumptions.

## 13. Animator contract

**HW:** The player uses an actual Animator Controller with states named `Run`, `Jump`, `Hit`, and `Death`. Changing sprites/colors from scripts alone does not replace these states. Each state needs an assigned, valid clip; placeholder clips must still show distinguishable behavior.

Default parameter contract:

| Parameter | Type | Meaning |
| --- | --- | --- |
| `IsGrounded` | Bool | Grounding state from player physics |
| `VerticalVelocity` | Float | Current vertical velocity, available for jump/landing presentation |
| `IsHit` | Bool | Nonfatal hit recovery is active |
| `IsDead` | Bool | Player has died this run; highest priority |
| `IsDashing` | Bool, optional | Visual dash feedback without replacing required states |

Use an existing controller's equivalent parameters if already wired and document the exact mapping. Cache hashes. Validate that scripts/controller agree on parameter names/types.

Transition requirements:

- Run to Jump when airborne; Jump to Run on valid landing.
- Run/Jump to Hit on accepted nonfatal damage. Hit returns to Run or Jump after recovery according to the actual grounding state.
- Any living state to Death when IsDead becomes true. Death has priority over Hit; stale Hit state cannot override it.
- Death has no automatic transition back into a living state. Restart explicitly resets/rebinds parameters/state before a new run.
- Run loops. Hit and Death do not loop unintentionally. Set exit times and transition durations deliberately; no long exit-time delay before Jump/Hit/Death.
- Disable Any State self-transition loops where they would restart Hit/Death continuously.
- Use no root motion that conflicts with Rigidbody2D physics or fixed horizontal anchoring.
- Gameplay Animator uses normal scaled timing so pause freezes the exact animation. GameOver keeps timeScale 1 and freezes gameplay separately so Death completes.
- Do not make damage/death correctness depend on animation events. Missing clips or skipped events cannot leave the run stuck in Playing.

Temporary hit flash captures/restores the original sprite color. It cannot leave a permanently tinted sprite after repeated hits, death, Restart, disable, or returning to the menu. Keep flash timing shorter than invulnerability; optionally blink during the remaining invulnerability time.

## 14. Score and PlayerPrefs persistence

`ScoreTracker` is the current score authority. Keep distance points and item points conceptually separate so each is counted once. Current score is a nonnegative integer using the formula from section 3; distance accumulation uses sufficient precision and floors accumulated distance rather than flooring each tiny frame increment.

- Reset current score/distance on a new run, but never reset high score during normal Play/Restart/Main Menu transitions.
- Publish a score update when the displayed integer changes or a coin is collected. HUD does not need to format identical values every frame.
- Live high score is `max(savedHighScore, highestCurrentScoreSeen)`. Update it immediately when the current run exceeds the record.
- Damage does not subtract score. Pause/GameOver stops distance gain. A collected coin adds its configured points exactly once.
- Ignore score requests after death or from already collected items. Guard accidental negative item values and integer overflow rather than allowing a wrapped negative display.

Persistence contract:

1. Use PlayerPrefs as required, behind a small high-score store boundary so tests can substitute an in-memory store where useful.
2. Define one namespaced constant key, initially `ChromeDino.HighScore.v1`. Load with a default of zero; treat invalid negative stored values as zero.
3. Keep the live record in memory. Write/flush a dirty record at GameOver, explicit in-game Pause, leaving the run, Quit, and application suspension/focus loss as appropriate. Flush before starting over with a lower current score.
4. Use `PlayerPrefs.SetInt` and `PlayerPrefs.Save` at those checkpoints; do not save every scoring frame. Save only when there is a dirty record.
5. Never call `PlayerPrefs.DeleteAll` in gameplay or tests. Tests use an isolated key and remove only that key, restoring any previous value if required.
6. Store only the assignment's high score by default; no database, cloud save, account, or encrypted save infrastructure.
7. Verify persistence by closing and reopening a standalone build with the same company/product identifiers. Editor and build preference locations are not assumed to be the same.

## 15. Canvas, menus, and button behavior

Use a Canvas with a sensible scaler and anchors. Preserve current UI assets/layout where possible. The coding work must supply and document all bindings.

| Screen/panel | Required elements and behavior |
| --- | --- |
| Main Menu | Play, Quit, control hints; high score display is useful but optional |
| Gameplay HUD | Current Score, High Score, health bar, visible Pause button |
| Pause Menu | Separate panel with Resume, Restart, Main Menu; stops gameplay while keeping UI responsive |
| GameOver | Final Score, High Score, Restart, Main Menu; health bar shows zero |

Health bar: use a Slider with min=0, max=3, whole numbers, or a filled Image with `fillAmount = lives / 3f`. A text label or hearts may supplement it but must not replace the required bar.

- Show initial values before the first playable frame. HUD remains visible during Playing and may remain behind pause/game-over overlays.
- Bind HUD to health/score events and explicitly refresh when re-enabled; no UI polling of scene-wide searches.
- One active EventSystem. Configure its `InputSystemUIInputModule` with working UI actions and appropriate first-selected buttons for keyboard navigation.
- Opening a panel selects an appropriate button; closing it clears stale selection where necessary. Gameplay cannot activate a hidden menu button.
- Button callbacks route into GameSession; no duplicate Inspector listener plus code listener for the same command.
- Main Menu and Pause Menu are distinct panels/screens, even if they share a Canvas.
- `Application.Quit` is valid in standalone builds; in the Editor document that it does not close the Editor. Do not put unguarded UnityEditor calls in runtime scripts.
- Confirm the layout stays readable at the chosen desktop resolution and at least one other aspect ratio.

## 16. Unity integration checklist

Deliver a concise `Docs/UnitySetup.md` with the actual object/asset names and the following mapping. If setup can be completed in Unity, complete it and verify it. If the Editor is unavailable, list exact pending steps; do not claim the assignment is playable yet.

| Target | Components/settings and references to verify |
| --- | --- |
| Scene root / session | GameSession, settings, player, world, spawner, score/persistence, HUD/menu references |
| Player root | Dynamic Rigidbody2D, player Collider2D, PlayerController, PlayerHealth; references to input/session/settings |
| Player visual child | SpriteRenderer, Animator with controller, animation/feedback components; no conflicting root motion |
| Feet check | Ground-check Transform and radius/distance; Ground layer mask only |
| Ground | Correct Ground layer and continuous non-trigger Collider2D |
| Ground obstacle prefab | Concrete GroundObstacle deriving from Interactable, trigger Collider2D, world mover/appropriate Rigidbody2D, visible sprite |
| Aerial obstacle prefab | Concrete AerialObstacle deriving from Interactable, trigger Collider2D, mover/appropriate Rigidbody2D, distinct height/sprite |
| Coin prefab | CoinCollectible deriving from Interactable, trigger Collider2D, mover, visible sprite, valid points |
| Spawn controller | All three prefab references, spawn heights, grace period, spacing limits, cleanup boundary |
| Background | Two or more parallax layers with enough tiles, different factors, and correct wrap width |
| Camera | Orthographic projection, playable framing, right spawn edge, left cleanup edge |
| Canvas/HUD | Score/high-score text, bar configuration, Pause button; correct anchors/scaler |
| Menu panels | Separate Main/Pause/GameOver panels, command callbacks, initial visibility, keyboard selection |
| EventSystem | Exactly one active EventSystem; InputSystemUIInputModule, UI action references; no StandaloneInputModule |
| Project input settings | Active Input Handling = Input System Package (New) |
| Animator Controller | Four required states, clips, parameters, valid transitions, death priority |
| Build configuration | Correct starting scene included in the Editor version's Build Settings/Build Profiles; valid desktop target/module |
| Version control | Assets and their Unity-generated `.meta` files, Packages, ProjectSettings, documentation, supplied `.gitignore` |

Prefer normal scene/prefab editing through Unity. A small idempotent Editor setup/validation command is acceptable if it saves genuine repetitive work. It must use Unity APIs, keep Editor code out of player assemblies, preserve existing assets, and avoid duplicate objects/listeners on rerun. Do not handwrite guessed Unity YAML/GUIDs or construct the whole production UI every frame from a monolithic script.

## 17. Verification and acceptance tests

Implement focused tests for gameplay invariants and lifecycle bugs. Do not pad coverage with tests that merely check field names or mirror setter implementations. Use Unity Test Framework rather than adding a test framework through NuGet. Tests must be independent and restore timeScale, created objects, seeded random state if changed, and isolated preferences.

Suggested Edit Mode coverage:

| Test | Expected result |
| --- | --- |
| New run/reset | Lives=3, current score=0, high score retained |
| Accepted damage and death | Separate eligible hits yield 3->2->1->0; one death event; no negative lives |
| Encounter/invulnerability guard | Same encounter cannot damage twice; a distinct encounter after invulnerability can |
| Score arithmetic | Distance fractions accumulate correctly; one coin adds 10; total/high-score values match |
| High-score comparison | Lower subsequent score never overwrites a higher record |
| Persistence store | Missing/negative key reads safely; valid new record survives a new store instance; unrelated keys remain untouched |
| State transition guards | Invalid/repeated transitions do not duplicate reset/death or resume an ended run |

Use pure model tests where it naturally simplifies timing; do not introduce a large abstraction framework just to turn every MonoBehaviour into a pure class.

Suggested Play Mode / integration coverage:

| Test | Expected result |
| --- | --- |
| Space action | One grounded press jumps; airborne/held press does not repeatedly jump |
| Shift action | One press creates the configured burst; cooldown rejects another dash; speed returns to base |
| Ground/aerial contact | Both concrete obstacle types remove one life and show Hit feedback |
| Duplicate contacts | Multiple colliders/callbacks for one obstacle cannot subtract multiple lives |
| Coin overlap | Coin adds score once and disables/despawns |
| Pause during movement | World, spawn countdown, score, jump physics, dash/cooldown, flash/invulnerability, and animation remain frozen |
| Resume and state input | UI works at timeScale zero; no queued/held gameplay action fires on Resume |
| Fatal hit | Score/world stop immediately; Death state can finish; GameOver appears exactly once |
| Restart/menu cycles | Repeated transitions restore health/physics/tint/Animator/timeScale and do not double callbacks |
| Maximum-speed contact | Dash cannot tunnel through either hazard without invoking the guarded interaction |

Use the Input System's available test helpers if supported by the installed test package/assembly setup, or another documented way to drive actions. Test failures must not be hidden by disabling test files or weakening core requirements.

Manual acceptance checklist:

- [ ] A clean checkout opens with the recorded Unity version and resolves packages.
- [ ] No compile errors, missing script components, null-reference exceptions, or project-generated runtime warning spam.
- [ ] Main Menu starts a run, showing health=3 and current score=0 immediately.
- [ ] Space jumps; Shift visibly dashes; movement works at different frame rates.
- [ ] Both hazard types and coins appear in the actual gameplay scene.
- [ ] Avoidable random layouts remain fair at maximum dash speed.
- [ ] Every valid hazard encounter removes exactly one life.
- [ ] Run/Jump/Hit/Death transitions and temporary hit flash are visible.
- [ ] Pause and Resume work through Escape and UI; paused gameplay does not advance.
- [ ] Final hit triggers GameOver; Death finishes without the world continuing.
- [ ] Restart works repeatedly, including from Pause; no duplicate input or stale temporary state.
- [ ] Current Score and High Score update live; health bar reflects all four life values.
- [ ] High score survives closing/reopening the standalone build, including leaving a run early via menu/Quit.
- [ ] Parallax wraps without visible gaps at two aspect ratios and during dash.
- [ ] Several minutes of gameplay do not leave unlimited active/spawned objects behind.
- [ ] Standalone build launches from the intended scene and includes all required runtime files.
- [ ] All mandatory requirement IDs have an implementation and verification entry.

Perform a targeted legacy-input audit on authored game scripts, for example:

```bash
rg -n 'UnityEngine\.Input\b|\bInput\.(GetKey|GetButton|GetAxis|GetMouseButton|mousePosition|touchCount)|StandaloneInputModule' Assets/_Project
```

Inspect each match and also check scenes/prefabs/settings; a text search alone does not prove correct input configuration. Do not modify third-party package internals because they contain compatibility code.

Use the Unity Editor/Test Runner or actual supported batch-mode commands to compile/test/build. Record the exact commands and results. Do not claim `dotnet build` of generated Unity project files proves scene/prefab/input/build correctness. If Unity or a target module/license is unavailable, perform useful static checks, finish the source work, and clearly mark Editor-dependent tests/builds pending. Never report unexecuted tests as passing.

## 18. Implementation workflow and completion report

Work in manageable stages and verify the relevant behavior before adding optional polish:

1. Inspect repository/version/packages and record the concrete integration plan.
2. Establish settings, state transitions, run reset, and persistence boundaries.
3. Configure New Input System gameplay/UI actions and implement grounded jump/dash.
4. Implement Interactable inheritance, hazards, coins, health, and score.
5. Implement scrolling/parallax and bounded, fair spawning.
6. Wire Animator/feedback, HUD, menus, and scene/prefab references.
7. Run focused tests and manual acceptance checks; fix discovered defects.
8. Document setup, dependencies, architecture, requirement coverage, and build/submission steps.

Deliver actual source files and required assets/configuration edits, not only explanations. Preserve existing implementations where they already satisfy this brief. Make justified local improvements without expanding scope.

At completion, report:

- What changed and where the main systems live.
- Exact Editor and dependency versions used.
- Which tests/checks/builds ran, their results, and any relevant logs.
- HW-01 through HW-11 coverage, with scene/script evidence and tested/manual/pending status.
- Any concrete remaining Inspector/art/build step, with exact object/field/asset names.
- Brief explanation of the class inheritance and gameplay flow that a student can understand and present.

Do not automatically push, publish, or submit to the course platform without the user's instruction. Local implementation and verification should proceed without repeated permission requests for routine reversible work.

## 19. Course rules, Git, and submission

These requirements come from the homework, rather than from Unity's APIs:

- The stated deadline is **26 Mehr 1405**. Do not invent an extension or change the source date.
- Discussion/collaboration is allowed, but each person's submitted answer must be written by that person. Document assistance and external references as required; the student must review, understand, and take responsibility for the final submission. Do not represent generated work as unaided authorship.
- Include names of collaborators and addresses of external sources used. Record Codex assistance transparently where course policy requires it; do not invent collaborator names.
- Put the Unity project in a Git repository such as GitHub or GitLab.
- Use the supplied `.gitignore` from Appendix A at the Unity project root. If a root ignore file already exists, preserve unrelated legitimate rules while incorporating the supplied Unity rules. Do not replace it with a different template merely because it is newer.
- Track required source/assets, their `.meta` files, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/`, and documentation. Do not globally ignore `.meta` files; the supplied file only ignores specific generated debug `.meta` files.
- Exclude Library, Temp, build output, logs, IDE/generated solution files, and other paths specified by that `.gitignore`. Check Git tracking, because adding an ignore rule does not untrack already committed files. Do not delete local project data to remove generated paths from Git.
- The final ZIP filename pattern is `GD_Hw1_[firstName]_[lastName]_[StudentId].zip`. Fill identity fields only from verified user-provided data.
- The final ZIP includes: a text file containing the project repository link, the executable game build, and a short work report explaining implementation structure with screenshots of the final game.
- Include the entire runnable build output required by the selected platform, not just a launcher executable without its data/runtime folders. Keep builds out of Git while including them in the submission ZIP.
- A C#-only source delivery is one part of the assignment; it does not replace the repository/build/report requirements. Codex should prepare what is possible and identify any remaining submission work explicitly.

Suggested accompanying documents: `README.md` for running/setup/dependencies, `Docs/UnitySetup.md` for wiring, `Docs/RequirementCoverage.md` for HW coverage and validation, and a short report/source acknowledgment suitable for submission. Combine documents if that keeps a small project clearer.

## 20. Official technical references

Use documentation matching the actual Editor and installed package versions. These references ground package/input/persistence decisions; they do not prescribe an upgrade or a specific package version for this project.

- [Unity Input System installation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/Installation.html) - package installation and Active Input Handling; this is a versioned reference, not a request to install 1.4.
- [Unity Input System UI support](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/UISupport.html) - InputSystemUIInputModule and UI actions; use the equivalent page for the installed version.
- [Unity package lockfiles](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-conflicts-auto.html) - deterministic resolution in `packages-lock.json`.
- [Unity PlayerPrefs.Save](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.Save.html) - explicit preference flushes and avoiding repeated writes during gameplay.
- [Unity Time.timeScale](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time-timeScale.html) - scaled time, paused FixedUpdate, and scaled coroutine behavior.
- [Unity 6 Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html) - version-specific physics property. Do not copy the legacy-input sample on that API page into this project.

## Appendix A. Supplied Unity `.gitignore`

Copy this supplied content into the Unity project's root `.gitignore`. Preserve it as the assignment baseline; add only relevant existing/project-specific rules. A comment linking to a newer template is not an instruction to replace this supplied baseline.


```gitignore
# This .gitignore file should be placed at the root of your Unity project directory
#
# Get latest from https://github.com/github/gitignore/blob/main/Unity.gitignore
#
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/

# MemoryCaptures can get excessive in size.
# They also could contain extremely sensitive data
/[Mm]emoryCaptures/

# Recordings can get excessive in size
/[Rr]ecordings/

# Uncomment this line if you wish to ignore the asset store tools plugin
# /[Aa]ssets/AssetStoreTools*

# Autogenerated Jetbrains Rider plugin
/[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.aab
*.unitypackage
*.app

# Crashlytics generated file
crashlytics-build.properties

# Packed Addressables
/[Aa]ssets/[Aa]ddressable[Aa]ssets[Dd]ata/*/*.bin*

# Temporary auto-generated Android Assets
/[Aa]ssets/[Ss]treamingAssets/aa.meta
/[Aa]ssets/[Ss]treamingAssets/aa/*
```
