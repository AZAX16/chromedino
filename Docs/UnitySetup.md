# Unity setup

Target: **Unity 6000.6.3f1 (45d8eee7de74)**. The existing game scene, prefabs, animation, Inspector references, UI, and two parallax layers are already authored. Preserve them; the creation instructions below are a reference for rebuilding or checking setup, not a request to recreate completed work. The score/difficulty update preserves `Assets/Scenes/Game.unity` and `SampleScene.unity` and edits only the referenced settings asset's coin reward and new difficulty fields.

The coin-pattern update also preserves both scenes and all prefabs/art. Its seven pattern settings are serialized in the same assigned asset; spawning, tests, and documentation are updated without scene reconstruction.

## 1. Import and project settings

1. Open this project with the recorded Editor, let UPM resolve `Packages/manifest.json`, and wait for compilation. No package installation or upgrade is required. Keep the Unity-generated lockfile.
2. In Player settings, Active Input Handling = **Input System Package (New)**; this is already `activeInputHandler: 1`. Use the default dynamic Input System update mode (Process Events In Dynamic Update).
3. Preserve the existing Universal Render Pipeline 17.6.0 / 2D renderer. Use Sprite-Unlit-Default for placeholders, or Sprite-Lit-Default with the existing Global Light 2D.
4. Import **TMP Essential Resources** when Unity prompts while creating the first TextMeshPro UI text. uGUI 2.6.0 already includes TMP; do not install another TMP package.
5. Keep Physics2D gravity `(0, -9.81)` and fixed timestep `0.02` as initial values. The session never changes the fixed timestep. Spawning validates downward gravity; changing gravity requires retuning jumps and spacing.
6. Create the following GameObject layers (their numerical slots are your choice): `Ground`, `Player`, `Hazard`, `Collectible`. Set every player collider child to Player too.
7. In **Physics 2D > Layer Collision Matrix**, enable Player/Ground, Player/Hazard, Player/Collectible. Disable Hazard/Hazard, Hazard/Collectible, Collectible/Collectible, Hazard/Ground, Collectible/Ground. Ground checks use Ground only. No tags are required.
8. Suggested Sorting Layers: `BackgroundFar`, `BackgroundNear`, `GroundArt`, `Interactables`, `Player`. Assign increasing foreground order, with UI on its Canvas.

## 2. Assets and input

The current GameSession references **`Assets/Settings/DefaultRunnerSettings.asset`**. Reuse that asset. For a future scene needing its own settings, use **Create > Chrome Dino > Runner Settings**; newly created assets default to 50 coin points, a 500-point difficulty threshold, and a 1.25 harder-speed multiplier.

The delivered input asset is `Assets/_Project/Input/RunnerControls.inputactions`. Leave **Generate C# Class off**; runtime code uses serialized InputActionReferences and needs no generated wrapper. The KeyboardMouse scheme requires Keyboard and Mouse. Maps/actions:

| Map/action | Action type / control | Bindings |
| --- | --- | --- |
| Gameplay/Jump | Button / Button | Space |
| Gameplay/Dash | Button / Button | Left Shift, Right Shift |
| System/Pause | Button / Button | Escape |
| UI/Point | Pass Through / Vector2 | Mouse position |
| UI/Click | Pass Through / Button | Mouse left button |
| UI/Navigate | Pass Through / Vector2 | 2DVector composite: Up/Down/Left/Right arrows |
| UI/Submit | Button / Button | Enter, Numpad Enter |
| UI/Cancel | Button / Button | Backspace |

Button actions perform once on press with the Input System's default press behavior. Escape has exactly one handler, `RunnerInputReader` -> `GameSession`; it is deliberately absent from UI/Cancel. Backspace is ordinary UI cancel, without a second session toggle handler. Space is absent from Submit. RunnerInputReader leaves its three actions enabled in menus to observe releases, suppresses gameplay requests there, and disables only actions it enabled when the component is disabled. UI actions belong to the UI module. Do not add PlayerInput, a generated callback wrapper, or another input listener.

The template's `Assets/Settings/InputSystem_Actions.inputactions` is preserved. In **Project Settings > Input System Package > Project-wide Actions**, select **RunnerControls** instead, so the template asset does not become the UI default. Save the project setting through Unity.

## 3. Scene hierarchy and components

Use `Assets/Scenes/Game.unity` as the intended game scene. Suggested names below are the concrete names used by this checklist; scripts use the Inspector references, not name searches. Keep all system components and Player active throughout the scene. **Do not put session, input, HUD controller, or menu controller under a panel that gets hidden.**

```text
GameSystems                  GameSession, RunnerInputReader, ScoreTracker,
                             WorldScrollController, SpawnController
Main Camera                  Camera (existing URP camera components retained)
Global Light 2D              existing, if using lit sprites
Ground                       BoxCollider2D (non-trigger)
Player                       Rigidbody2D, Capsule/BoxCollider2D,
                             PlayerController, PlayerHealth
  FeetCheck                  Transform
  Visual                     SpriteRenderer, Animator,
                             PlayerAnimationController, PlayerHitFeedback
SpawnHeights
  GroundSpawnHeight          Transform
  AerialSpawnHeight          Transform
  CoinSpawnHeight            Transform
SpawnedObjects               empty Transform, scale (1,1,1)
BackgroundFar                ParallaxLayer, repeated sprite tile children
BackgroundNear               ParallaxLayer, repeated sprite tile children
Canvas                       Canvas, CanvasScaler, GraphicRaycaster,
                             HudController, MenuController
  MainMenuPanel              Play, Quit, English control hints
  HUDPanel                   ScoreText, HighScoreText, HealthBar, PauseButton
  PauseMenuPanel             Resume, Restart, MainMenu
  GameOverPanel              FinalScoreText, HighScoreText, Restart, MainMenu
EventSystem                  EventSystem, InputSystemUIInputModule
```

### GameSystems Inspector

| Component / field | Assign |
| --- | --- |
| GameSession / Settings | `Assets/Settings/DefaultRunnerSettings.asset` (existing assigned asset) |
| GameSession / Input | GameSystems RunnerInputReader |
| GameSession / Player | Player PlayerController |
| GameSession / Animation | Player/Visual PlayerAnimationController |
| GameSession / Feedback | Player/Visual PlayerHitFeedback |
| GameSession / World | GameSystems WorldScrollController |
| GameSession / Spawner | GameSystems SpawnController |
| GameSession / Parallax Layers | Array with BackgroundFar and BackgroundNear, each once |
| GameSession / Camera | Main Camera Camera |
| GameSession / Score | GameSystems ScoreTracker |
| GameSession / HUD | Canvas HudController |
| GameSession / Menus | Canvas MenuController |
| RunnerInputReader / Jump | RunnerControls > Gameplay > Jump |
| RunnerInputReader / Dash | RunnerControls > Gameplay > Dash |
| RunnerInputReader / Pause | RunnerControls > System > Pause |

`ScoreTracker` and `WorldScrollController` have no Inspector fields. `PlayerPrefsHighScoreStore` is an ordinary C# service constructed by GameSession; do not try to attach it. GameSession initializes other systems explicitly in Start. No Script Execution Order setting is needed, and no persistent singleton or bootstrap scene is needed.

### Camera, floor, and player

Starting layout in world units: camera `(0, 2.5, -10)`, orthographic size `5`; floor top at `y=0`; Player root `(-5, 0.5, 0)` using a `0.7 x 1` collider. Keep camera orientation `(0,0,0)` and position fixed. Test 16:9 and 4:3; camera-relative spawn/cleanup X positions are recomputed as the aspect changes.

Ground: layer Ground, a solid BoxCollider2D with top `y=0`, width at least the entire visible area plus margins (e.g. width 100, height 1, center y=-0.5). Leave this collider stationary. Scrolling floor artwork can be a separate parallax layer with factor 1.

| Component / field | Configure |
| --- | --- |
| Player Transform | Unit scale, position above floor, no transformed moving parent |
| Player layer | Player, including any collider children |
| Rigidbody2D | Dynamic, Simulated on, Gravity Scale 3, Linear Damping 0, Angular Damping 0, Freeze Position X and Rotation Z, Continuous collision detection, Interpolate |
| Player collider | Solid (Is Trigger off), snug around sprite, no bouncy material |
| PlayerController / Body Collider | Player's main solid collider |
| PlayerController / Feet Check | Player/FeetCheck, at bottom of main collider |
| PlayerController / Ground Check Radius | 0.08 world units initially; its green gizmo should just touch Ground |
| PlayerController / Ground Layers | **Ground only**; exclude Player/Hazard/Collectible |
| PlayerHealth | No Inspector fields; exactly 3 starting/max lives, always |
| Visual SpriteRenderer | Player sprite, Player sorting layer, capture desired starting Color |
| Visual Animator | RunnerAnimator controller below; Apply Root Motion off, Update Mode Normal, Culling Mode Always Animate, Speed 1 |
| PlayerAnimationController | No Inspector fields; GameSession supplies Player |
| PlayerHitFeedback | No Inspector fields; same object as the SpriteRenderer; GameSession supplies Player/settings |

Do not animate Transform position on the Player root, SpriteRenderer Color (which would compete with flash/dash), Rigidbody2D settings, or gameplay variables. Animate the Visual child sprite/local pose. GameSession restores the starting root pose, zero velocity, gravity, fixed X/rotation constraints, tint, Animator, and all run timers on restart. Menus freeze the body; pause preserves its exact velocity using timeScale zero. GameOver freezes the body with timeScale one so Death can finish.

### Interactable prefabs

Create three prefabs through Unity, under `Assets/_Project/Prefabs`: `GroundObstacle`, `AerialObstacle`, `Coin`. Each has **one root BoxCollider2D**, **one root Kinematic Rigidbody2D**, **WorldMover**, a visible SpriteRenderer (root or visual child), and exactly one concrete script from the table. Do not add Obstacle or Interactable separately; they are abstract bases. Do not add independent damage/collection scripts or event callbacks.

| Prefab | Layer / concrete script | Initial centered collider size | Initial spawn root Y |
| --- | --- | --- | --- |
| GroundObstacle | Hazard / GroundObstacle | (0.5, 0.8) | 0.4, bottom at floor |
| AerialObstacle | Hazard / AerialObstacle | (0.7, 0.5) | 2.0, safely above standing player, inside jump path |
| Coin | Collectible / CoinCollectible | (0.3, 0.3) | 1.2, reachable during jump |

For each prefab: active root and enabled scripts/collider; positive unit scale; identity rotation; BoxCollider2D Is Trigger on, Offset X=0; Rigidbody2D Simulated on, Kinematic, Gravity Scale 0, Freeze Rotation Z, Interpolate, Continuous where available. Give aerial hazards a distinct sprite/silhouette. Set every child collider, if later added, to the root's layer, but keep the single root collider for the authored sweep geometry. `WorldMover` and the concrete scripts have **no Inspector fields**; session/player/collider references are initialized after spawning. Use prefab assets for spawner references, not scene instances.

Trigger callbacks are dispatched from the player's Rigidbody2D root, including contacts involving its child colliders. WorldMover also overlaps/casts its root box against Player before moving, using the same guarded polymorphic interaction. A long overlap or duplicate callback consumes an encounter once, including during invulnerability. Dash grants no immunity. Test actual collider geometry at maximum dash speed.

### SpawnController Inspector

| Field | Assign |
| --- | --- |
| Camera | Main Camera |
| Ground Obstacle Prefab | GroundObstacle prefab asset |
| Aerial Obstacle Prefab | AerialObstacle prefab asset |
| Coin Prefab | Coin prefab asset |
| Ground Spawn Height | SpawnHeights/GroundSpawnHeight at y=0.4 |
| Aerial Spawn Height | SpawnHeights/AerialSpawnHeight at y=2.0 |
| Coin Spawn Height | SpawnHeights/CoinSpawnHeight at y=1.2 |
| Spawned Objects Root | SpawnedObjects, world scale (1,1,1), identity rotation |

Only the anchors' Y coordinates are used. X comes from the camera's right edge plus margin, object width, and reaction distance. Cleanup X comes from its left edge minus margin. The first spawn waits at least 2 gameplay seconds. The spawner cycles Ground -> Aerial -> Coin pattern and randomizes the proposed interval; geometric spacing can lengthen the wait. Each coin turn selects one weighted pattern with the same seeded `System.Random` used for intervals. A pattern consumes one opportunity and creates either one or three ordinary `CoinCollectible` instances. **Hazard clearance and reaction boundaries always use the global hardest-stage dash speed (15 with current settings), even for objects spawned below 500 points.** Ground-hazard jump clearance is still validated at the slowest normal speed, 6, where clearing its width takes longest. `ResetRun(seed)` is available for deterministic tests and is not exposed as an in-game cheat.

### Coin patterns and full-pattern clearance

- **Single:** preserves the existing Coin Spawn Height anchor and prefab, including its collider offset.
- **Row:** three coins at the initial standing collider's center Y, separated by Coin Row Spacing World Units (at least coin collider width + 0.2). Default separation is 1 world unit. Run through all three for exactly 150 item points, plus ordinary distance points accrued while moving.
- **Arc:** three coins sampling one grounded jump around its apex. With gravity magnitude `g = -Physics2D.gravity.y * GravityScale`, use `v = JumpVelocity - g*fixedDeltaTime/2` to account for Unity applying gravity before position. Apex time is `v/g`. Neighbors are one `apexTime * CoinArcHalfDurationFraction` before/after the peak. Horizontal separation is that duration times the current normal stage speed (6 or 7.5); vertical centers follow `standingColliderCenterY + v*t - g*t*t/2`. Dash does not stretch the pattern. The layout stays fixed after spawning and supports a stage change during approach/collection at the delivered physics and geometry.
- **Repeated jumps:** three coins at the standing collider center plus jump apex height. Center separation is `max(playerWidth + coinWidth + 0.2, fastestNormalSpeed * (fullFlightSeconds + LandingRecoverySeconds + RepeatedJumpReactionSeconds))`, where `fastestNormalSpeed = BaseSpeed * HarderSpeedMultiplier` (7.5). This keeps a compact, predictable jump rhythm while covering a difficulty change after spawning. Each encounter allows landing, releasing Space, and a new grounded press; holding Space continues to produce only one jump. A deliberately triggered dash may miss optional coins. Hazard spacing outside the pattern still uses maximum dash speed independently. No airborne jump or automatic jump is added.

The authored Game scene uses a **0.8 x 1.2 capsule**, standing root/center at **(-4, -1.9)**, and a **0.35 x 0.35** coin collider. Runtime measures collider size/offset and standing center during initialization. With Jump Velocity 10, Gravity Scale 3, gravity -9.81 and fixed timestep 0.02, nominal continuous flight time is about **0.680 s**; the discrete apex is about **1.600** units above standing center. The default arc has approximately **0.791 / 0.989** center separation at the two normal speeds and a **0.256** rise from either edge to the peak. Repeated-jump center separation is now about **8.097** (previously 24.094), allowing about **1.349 / 1.080 s** between coin centers at normal speeds, including at least **0.4 s** beyond nominal flight for landing/release/reaction at the harder stage. These values are derived, not additional Inspector settings. Unity's discrete flight and real capsule contacts are covered by production-scene Play Mode tests.

Hazard clearance is measured from the **last coin's right collider edge**, preserving the full pattern span, to the following opportunity's first left edge. The edge gap includes player width plus a complete flight, landing recovery, and reaction allowance at maximum dash speed. The same allowance precedes a pattern. This reservation survives collection/despawning and uses accumulated shared world distance, so missing coins, a difficulty change, partial dashes, or camera aspect/boundary changes cannot shorten it. Ordinary row/arc neighbors have only their own small separation. Repeated-jump neighbors use the separate normal-speed jump cadence. Optional coins can be missed during a dash; hazards remain separated for the maximum speed.

The active-object cap counts individual coins. A selected three-coin pattern waits until all three slots are free; it never truncates or rerolls into a smaller pattern. If the configured total cap itself is below three, an oversized pattern opportunity is skipped so hazard rotation can continue; use a cap of at least three to enable all patterns. Selection, reserved space, timers, and rotation are cleared on restart, Main Menu, and disable. Every coin uses the existing guarded scoring and ordinary despawn path, with no extra Update or manager.

**Inspector action:** let Unity reimport the scripts, then select the already-assigned **Assets/Settings/DefaultRunnerSettings.asset** and verify the seven coin-pattern fields in the table below, including **Repeated Jump Reaction Seconds = 0.15**. They are serialized in the delivered asset; no new component/reference, anchor move, prefab edit, or scene save is needed. Increase this new field for more time between repeated jumps or lower it to a minimum of 0.1 for tighter timing. It adds to Landing Recovery Seconds and does not change hazard reaction spacing. Zero disables an individual pattern weight; all four weights zero falls back to Single. Changing physics, collider geometry, arc fraction, or speed requires repeating both automated and hands-on jump checks.

### ParallaxLayer Inspector (each layer)

Use at least **two visible layers**, e.g. distant mountains and nearer clouds/terrain. Each layer has identical, seamless sprite tiles with **left-center sprite pivots**. Keep tile Y/Z and scale equal and tile parents stationary. Use SpriteRenderer Sorting Layers to put them behind gameplay.

| Field | Configure |
| --- | --- |
| Tiles | Repeated tile Transforms, strictly left-to-right, no duplicates |
| Tile Width World Units | Actual rendered width including scale; e.g. 20, never raw sprite pixels |
| Speed Factor | BackgroundFar 0.2; BackgroundNear 0.5; optional floor art 1 |

With a 20-unit tile and a centered camera showing 17.78 units horizontally, tile left pivots `x=-10,10` cover the view. For a wider supported aspect ratio, add another tile (e.g. x=30). All tiles except one must cover the entire camera width; account for any extra left padding too. The right coverage test is `firstPivotX + (tileCount-1)*tileWidth >= cameraRight`. Configure enough tiles for the widest supported aspect ratio before playing. Code wraps the whole strip preserving overshoot and resets exact starting positions. Check seams while dashing and after many wraps.

## 4. RunnerSettings Inspector (every field)

All times are gameplay seconds and stop during pause; distances/speeds use world units.

| Inspector field | Initial value / purpose |
| --- | --- |
| Base Speed | 6 units/second |
| Jump Velocity | 10 units/second upward |
| Gravity Scale | 3; applied to Player each reset |
| Dash Speed Multiplier | 2; world, parallax and distance share the same integrated burst |
| Dash Duration Seconds | 0.20 |
| Dash Cooldown Seconds | 1.5 from dash end; accepted nonfatal damage cancels dash into cooldown |
| Difficulty Score Threshold | 500, total current-run points including coins; validated to at least 1 |
| Harder Speed Multiplier | 1.25, applied once at/above threshold; validated to a finite value at least 1 |
| Invulnerability Seconds | 1.0 after accepted nonfatal hit |
| Hit Recovery Seconds | 0.25; prevents new jumps/dashes, preserves current vertical motion |
| Hit Flash Seconds | 0.15; validated shorter than invulnerability |
| Hit Color | (1, 0.3, 0.3, 1), or a clearly visible flash for your sprite |
| Dash Color | (0.3, 0.85, 1, 1), visibly distinct burst feedback |
| Points Per World Unit | 1, applied to accumulated distance then floored |
| Coin Points | 50, awarded once per collected coin |
| First Spawn Delay Seconds | 2.0 minimum |
| Minimum Spawn Interval Seconds | 1.2, proposal before spacing constraints |
| Maximum Spawn Interval Seconds | 2.0, proposal before spacing constraints |
| Boundary Margin World Units | 2.0 beyond camera edges |
| Reaction Seconds | 0.6 at maximum speed |
| Landing Recovery Seconds | 0.25 for safe spacing |
| Minimum Clear Gap World Units | 12, increased by reachability calculation when needed |
| Maximum Active Interactables | 16 |
| Single Coin Weight | 1, relative chance on coin opportunities; 0 disables |
| Coin Row Weight | 2, relative chance; 0 disables |
| Coin Arc Weight | 2, relative chance; 0 disables |
| Repeated Jump Coin Weight | 1, relative chance; 0 disables |
| Coin Row Spacing World Units | 1, center-to-center row separation; increased to at least coin collider width + 0.2 |
| Coin Arc Half Duration Fraction | 0.4 of time-to-apex between neighboring arc coins; validated to 0.2–0.5 |
| Repeated Jump Reaction Seconds | 0.15 extra time between predictable coin jumps after flight and Landing Recovery Seconds; minimum 0.1, non-finite values reset to 0.15. Independent of hazard Reaction Seconds. |

Lives are a code constant at three and have no editable setting. Keep scoring/coin reward positive in the final assignment scene, and retain visible hit/dash feedback. Defaults assume unit-scale player geometry above. Retune layout when changing any physics value, obstacle width/height, or speed.

### Score difficulty and existing asset verification

The active serialized asset has already been updated; changing only C# initializers would leave its old reward in place. Outside Play Mode, open **Assets/Scenes/Game.unity**, select **GameSystems**, and click the **GameSession > Settings** reference to select **Assets/Settings/DefaultRunnerSettings.asset**. Verify **Coin Points = 50**, **Difficulty Score Threshold = 500**, and **Harder Speed Multiplier = 1.25**. No new component, scene reference, prefab change, or UI wiring is required. If a locally unsaved Inspector value differs, enter these three values on that assigned asset and save the asset/project; keep all other settings and wiring.

Below 500 total run points, normal/dash speeds are **6/12**. At 500 and above they become **7.5/15**, once per run, including when a 50-point coin crosses the threshold. `WorldScrollController` checks `ScoreTracker.CurrentScore` before each movement step; a collection from either a sweep or a trigger callback takes effect by the next physics step. Stored high score never activates difficulty. Runtime stage state belongs to the world controller and never writes the settings asset. Pause freezes movement and timers while retaining the current stage; Restart, a new run, returning to Main Menu, and session disable reset the stage and preserve high score.

`RunnerSettings.MaximumSpeed` is the global safety limit, **15**, throughout the run. `WorldScrollController.NormalSpeed` and `DashSpeed` describe the current stage, initially **6/12**. `EffectiveSpeed` reports the average speed used in the last completed `Tick` (zero while stopped; normal speed immediately after reset). A dash ending partway through a step contributes only its actual dash time, so that step's speed may lie between normal and dash speeds. `StepDistance = EffectiveSpeed * deltaSeconds`; obstacles, both parallax layers, and distance scoring consume that one integrated distance. Reading speed does not advance dash/cooldown timers.

## 5. Animator asset and transitions

Create `Assets/_Project/Animations/RunnerAnimator.controller` and four actual valid clips: `Run.anim`, `Jump.anim`, `Hit.anim`, `Death.anim`. Place these four states directly in **Base Layer**, named exactly **Run**, **Jump**, **Hit**, **Death**. Make Run the default. Even placeholder clips must look distinct: running leg/sprite cycle, airborne pose, recoil, and falling/dead pose. Sprite-only code does not satisfy this requirement.

Add **all five** parameters below; this implementation uses IsDashing for the Animator too:

| Parameter | Type |
| --- | --- |
| IsGrounded | Bool |
| VerticalVelocity | Float |
| IsHit | Bool |
| IsDead | Bool |
| IsDashing | Bool |

Use these transitions with Has Exit Time **off** and transition duration **0** initially. No root motion. VerticalVelocity is provided for optional rising/falling presentation; a single Jump clip is sufficient. IsDashing can control extra visual pose/blending while retaining all four required states; tint feedback is already implemented.

| Transition | Conditions |
| --- | --- |
| Any State -> Death (highest priority) | IsDead=true; Can Transition To Self off |
| Any State -> Hit | IsHit=true AND IsDead=false; Can Transition To Self off |
| Run -> Jump | IsGrounded=false AND IsHit=false AND IsDead=false |
| Jump -> Run | IsGrounded=true AND IsHit=false AND IsDead=false |
| Hit -> Run | IsHit=false AND IsGrounded=true AND IsDead=false |
| Hit -> Jump | IsHit=false AND IsGrounded=false AND IsDead=false |

Death has **no outgoing transitions**. Run loops; Hit and Death do not loop. Jump may hold/loop its airborne pose. Hit duration is driven by settings/IsHit, without dependence on an animation event. Death must remain visible long enough to finish behind the GameOver overlay. The session explicitly rebinds/resets Animator parameters and Run state on restart; it does not wait for an animation event to end a run. Use scaled Animator timing so pause stops it exactly.

## 6. Canvas, references, and manual buttons

Canvas: Screen Space - Overlay, CanvasScaler = Scale With Screen Size, initial reference resolution 1920x1080, Match 0.5; GraphicRaycaster enabled. Anchor HUD corners appropriately; make overlays stretch to the screen and panel labels readable at 16:9 and 4:3. Pause/GameOver overlays should block clicks through to underlying buttons. HealthBar is a **Slider with a Fill Rect and visible fill Image**, min=0, max=3, Whole Numbers on, Interactable off; remove its handle or hide it. Three lives is a full bar; zero is empty. Text/hearts alone do not replace the bar.

Keep HudController/MenuController on the always-active Canvas root. All score labels are **TextMeshProUGUI**, with a valid font asset. Do not attach HudController only to the HUDPanel.

| HudController field | Assign |
| --- | --- |
| Score Text | HUDPanel/ScoreText |
| High Score Text | HUDPanel/HighScoreText |
| Health Bar | HUDPanel/HealthBar Slider |
| Final Score Text | GameOverPanel/FinalScoreText |
| Game Over High Score Text | GameOverPanel/HighScoreText |

| MenuController field | Assign |
| --- | --- |
| Main Menu Panel | MainMenuPanel |
| Pause Menu Panel | PauseMenuPanel (distinct object) |
| Game Over Panel | GameOverPanel (distinct object) |
| HUD Panel | HUDPanel |
| Event System | The single EventSystem |
| Main Menu First Selected | MainMenuPanel/Play button GameObject |
| Pause Menu First Selected | PauseMenuPanel/Resume button GameObject |
| Game Over First Selected | GameOverPanel/Restart button GameObject |

Before Play Mode: MainMenuPanel active, other panels inactive; system objects/Canvas remain active. Code sets panel visibility, selection, and initial model values on startup. HUD remains visible under Pause and GameOver. Main Menu needs control hints: **Space: Jump | Shift: Dash | Escape: Pause/Resume | Arrows + Enter: Menu**.

Wire exactly **one Inspector On Click listener per button**, target GameSystems/GameSession, using these **public parameterless** methods. There is no code registration of these callbacks:

| Button | GameSession method |
| --- | --- |
| MainMenuPanel/Play | StartRun |
| MainMenuPanel/Quit | Quit |
| HUDPanel/PauseButton | Pause |
| PauseMenuPanel/Resume | Resume |
| PauseMenuPanel/Restart | Restart |
| PauseMenuPanel/MainMenu | ReturnToMainMenu |
| GameOverPanel/Restart | Restart |
| GameOverPanel/MainMenu | ReturnToMainMenu |

Set Button Navigation to Automatic or explicit visible neighbors. Ensure a decorative fullscreen Image does not intercept button raycasts. Opening a panel selects its configured button; entering Playing clears selection. Repeated/invalid commands are harmless. `Quit` works from MainMenu in standalone players; `Application.Quit` does not close the Unity Editor.

### EventSystem input module

Keep **one** EventSystem with **InputSystemUIInputModule**, remove StandaloneInputModule. Set EventSystem Send Navigation Events on. Assign the module's **Actions Asset = RunnerControls** and the following references manually, replacing all template/default references:

| InputSystemUIInputModule field | Action |
| --- | --- |
| Point | UI/Point |
| Left Click | UI/Click |
| Move | UI/Navigate |
| Submit | UI/Submit |
| Cancel | UI/Cancel |
| Middle Click, Right Click, Scroll Wheel | None (unused) |
| Tracked Device Position / Orientation | None (unused) |

The module handles pointer and navigation input at timeScale zero. Main Menu and Pause are separate panels. Do not bind Escape to UI/Cancel or add another pause callback. Do not use Space as UI/Submit. The menu controller selects buttons; it does not subscribe to button On Click or the Cancel action.

## 7. Validation, build, and delivery

Run the Edit Mode and Play Mode suites in **Window > General > Test Runner**, then do the visual/manual checks in [RequirementCoverage.md](RequirementCoverage.md). Most fixtures build disposable objects/controllers through Unity APIs with an inactive Canvas. `CoinPatternSceneTests` additionally loads the actual Game scene, uses its real capsule, coin prefab, input, session loop, and Unity physics, and unloads without saving. Tests use in-memory high scores or isolated preference keys. Production layout, UI navigation, font rendering, clips, seams, and standalone persistence still need manual verification.

For the coin-pattern update, run `CoinPatternTests`, `RunnerIntegrationTests`, and `CoinPatternSceneTests` with the existing full suites. Scene tests cover a ground row, a one-jump arc and three separate grounded jumps at both normal speeds, difficulty crossing on the first coin, a difficulty change on approach, and holding Space throughout a repeated sequence. Hands-on acceptance remains necessary: play all four patterns in Game.unity at both stages, check the visible row/arc spacing and comfortable timing, dash on approach/inside/after a pattern near both hazard types, pause mid-pattern, miss coins, and restart/return to menu. Test both 16:9 and 4:3. No build or publishing is required for this change.

Executed current coin-pattern validation in Unity 6000.6.3f1 after tightening repeated jumps: **32 Edit Mode and 37 Play Mode tests passed**, with zero failures/skips. Eleven Play Mode tests load the authored Game scene, including repeated-jump cadence at both normal speeds and across difficulty changes, and a speed-15 dash during an arc followed by safely jumping the next ground hazard. See [the spacing-adjustment verification record](RequirementCoverage.md#repeated-jump-spacing-adjustment-2026-10-26) for changed files, local logs/results, backup, and remaining manual checks.

For the 2026-10-26 score/difficulty update, Unity 6000.6.3f1 compiled an isolated copy of the current project and passed **17 Edit Mode tests and 19 Play Mode tests**, with no failures or skips. The active asset and its GameSession reference were verified through Unity. See [RequirementCoverage.md](RequirementCoverage.md#scoredifficulty-update-2026-10-26) for exact log/result paths, preservation checks, and the remaining manual checks.

Use **File > Build Profiles**, select a desktop target with its installed support module, and add `Assets/Scenes/Game.unity` as the enabled first scene. The preserved template configuration currently lists SampleScene; change it manually. Confirm Company Name/Product Name and keep them consistent between persistence checks. Build outside tracked source (e.g. `Builds/Windows/`). Quit/reopen the complete standalone output and confirm high score persists. Preferences use only `ChromeDino.HighScore.v1`; normal gameplay never deletes preferences. Focus loss/suspension pauses a playing run and flushes a dirty record; resuming focus requires an explicit Resume.

This folder was not a Git repository at inspection. Initialize/connect the desired repository yourself, track Assets and Unity-generated `.meta` files, Packages including lockfile, ProjectSettings, and docs. The supplied ignore baseline plus `*.slnx` is installed. Missing metadata for new assets was copied from the actual Unity-imported validation project; existing metadata was preserved and none was guessed. Let your Editor reimport these assets. Confirm generated directories are ignored before committing. Do not commit Library, Temp, Logs, builds, or generated IDE solutions.

Submission remains the student's responsibility: review/understand the code, fill verified identity/repository details, capture screenshots of the final configured game, build and test the full runnable output, and create `GD_Hw1_[firstName]_[lastName]_[StudentId].zip` containing the repository-link text file, complete build, and short work report. No identity, URL, screenshot, build, or submission is fabricated. The brief states the deadline as **26 Mehr 1405**.
