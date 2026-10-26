# Requirement coverage and verification

Implementation date: 2026-10-15 (user's Asia/Tehran date). Editor: **6000.6.3f1 (45d8eee7de74)**. Dependencies and render pipeline are unchanged. The production scene/Inspector/art setup belongs to the user, as requested. Source coverage and passing disposable tests do not mean the final game scene is configured or visually accepted.

## Homework mapping

| ID | Source / delivered evidence | Verification and remaining production work |
| --- | --- | --- |
| HW-01 | WorldScrollController, ParallaxLayer; shared integrated step distance, overshoot-preserving wrapping/reset | Automated repeated-wrap coverage test. User must assign at least two visible sprite layers with different factors and verify seams at two aspect ratios. |
| HW-02 | RunnerControls.inputactions; RunnerInputReader serialized references; New Input setting already enabled | Input System Play Mode tests drive Space/Shift/Escape, release gates, and UI action ownership. User must wire InputSystemUIInputModule and menu actions, replace template project-wide actions, and validate navigation/pointer input. |
| HW-03 | PlayerHealth, constant MaximumLives=3, HudController Slider binding | Tests cover reset and 3->2->1->0. User must create/assign the visible health bar and observe all four values. |
| HW-04 | Gameplay/Jump Space, Gameplay/Dash Shift; PlayerController single jump/dash/cooldown; world speed burst and sprite tint | Action/physics tests cover press/hold, speed restoration and cooldown rejection. User must tune feel and confirm visible feedback in actual sprites. |
| HW-05 | PlayerHealth accepted-hit authority; session death transition; Interactable encounter guard; swept contact fallback | Tests cover one accepted hit, invulnerability, fatal hit, repeated restart, and entire-player crossing in a step. User must test both actual prefab colliders at maximum dash speed. |
| HW-06 | Concrete GroundObstacle, AerialObstacle, CoinCollectible; SpawnController cycles all three with randomized proposed waits | Automated contact/coin and spawn-cap coverage. User must create all three visible prefabs and validate height, reachability, and spawn fairness during actual play. |
| HW-07 | GroundObstacle/AerialObstacle -> Obstacle -> Interactable; CoinCollectible -> Interactable | Shared guarded polymorphic entry point exercised by tests. Player does not branch by concrete interaction type. |
| HW-08 | PlayerAnimationController validates required Base Layer states and all five parameter types, translates state, and resets Animator | Disposable Play Mode controller exercises Death transition and reset. User must create Run/Jump/Hit/Death production clips/controller and verify all transitions and distinct visible behavior. |
| HW-09 | PlayerHitFeedback captures original color, flashes, restores on expiry/death/reset/disable; dash uses separate tint | Pause/tint/restart tested. User must confirm contrast and appearance on actual sprites. |
| HW-10 | HudController score/high-score/health events; MenuController distinct panels/selection; six public GameSession button methods | Session pause/menu/restart guards and HUD bindings exercised in fixture. User must wire Buttons manually, validate actual layout/UI at timeScale zero and two aspect ratios. No code onClick registration exists. |
| HW-11 | PlayerPrefsHighScoreStore, key ChromeDino.HighScore.v1, dirty checkpoints in GameSession/ScoreTracker | Isolated-key store and high-score/dirty-write tests passed. User must close/reopen the standalone build with unchanged company/product identifiers. Editor fresh-store tests do not prove cross-launch build persistence. |

## Automated checks

### GitHub handoff verification, 2026-10-27

The complete existing suites passed again in Unity **6000.6.3f1** before the initial `main` push to [AZAX16/chromedino](https://github.com/AZAX16/chromedino): **32 Edit Mode and 37 Play Mode tests passed, with zero failures or skips**. The isolated validation copy matched all 541 current Assets/Packages/ProjectSettings files, and the originals retained their hashes. No gameplay fixes were required. See [GitHubVerification.md](GitHubVerification.md) for results, reproduction instructions, handoff contents, and remaining manual checks. Earlier statements below about the folder being outside Git describe its historical state.

### Repeated-jump spacing adjustment, 2026-10-26

The repeated sequence previously reused hazard spacing, putting neighboring coins about **24.094 world units** apart. It now uses a separate predictable jump cadence: maximum normal stage speed (7.5) times nominal flight plus Landing Recovery Seconds (0.25) plus **Repeated Jump Reaction Seconds (0.15)**, with a small collider-separation floor. The delivered default is approximately **8.097 units**, about **66% less**. Encounters are about **1.349 s** apart at speed 6 and **1.080 s** at speed 7.5, leaving at least **0.4 s** beyond nominal flight for landing, release, and the next press. Using the harder normal speed also covers a difficulty change during approach or collection. Optional coins can still be missed during an intentional dash. Full-pattern hazard clearance retains the original global maximum dash speed and maneuver allowance.

The new setting is serialized on the assigned asset; verify **Repeated Jump Reaction Seconds = 0.15** in its Inspector after script import. Lowering it toward its minimum of 0.1 tightens timing; increasing it gives more time between jumps. No scene, prefab, input, movement, scoring, health, or persistence changes are required. The original mathematical test that required hazard-sized internal spacing was replaced with normal-speed flight/repress checks plus an upper limit on the wait; the full hazard safety assertions remain. Production-scene tests retain all collection/grounding/hold checks and now also require the first-to-third press to take under **2.9 s** at either normal stage. A new scene case covers acceleration before the first repeated coin.

Changed files: `Assets/_Project/Scripts/World/CoinPatterns.cs`, `Assets/_Project/Scripts/Configuration/RunnerSettings.cs`, `Assets/Settings/DefaultRunnerSettings.asset`, `Assets/_Project/Tests/EditMode/CoinPatternTests.cs`, `Assets/_Project/Tests/PlayMode/CoinPatternSceneTests.cs`, `Docs/UnitySetup.md`, and this file. Focused originals and hashes were saved before editing in **`Logs/Codex/RepeatedJumpSpacing-Before/`**. No packages, art, scene reconstruction, build, or publishing are part of this adjustment.

Unity **6000.6.3f1** ran the full suites in `Logs/Codex/CoinPatternsValidation/`, resynchronized from current Assets/Packages/ProjectSettings. Edit Mode ran with no graphics; Play Mode ran with graphics and the real Game scene. The earlier validation records below are historical.

| Check | Actual result / local evidence |
| --- | --- |
| Edit Mode | **32 passed, 0 failed, 0 skipped**, 0.1422193 s; `Logs/Codex/RepeatedJumpSpacing-EditMode.xml` and `.log`. Includes compact cadence at both normal speeds and independence from hazard spacing/dash multiplier. |
| Play Mode | **37 passed, 0 failed, 0 skipped**, 41.7262174 s; `Logs/Codex/RepeatedJumpSpacing-PlayMode.xml` and `.log`. |
| Authored scene | Repeated sequence collects all three coins with three separate grounded presses at 6 and 7.5 and across difficulty changes, with first-to-third press under 2.9 s. Holding Space still jumps/collects only once. Existing arc/row, speed-15 dash, hazard-clearance, cap, cleanup/reset, and gameplay regression checks pass. |
| Preservation | Scene/prefab/package/physics/timestep files retain their pre-edit hashes. Details: `Logs/Codex/RepeatedJumpSpacing-Preservation.json`. |

Remaining manual verification: judge the tighter rhythm and visible empty space in hands-on Game scene play at both stages and aspect ratios, including intentional dashes and pause/restart/menu transitions. Automated input/physics tests establish the tested collection and timing behavior; visual comfort still needs human playtesting. No build or standalone persistence rerun was performed.

### Coin-pattern update, 2026-10-26

Inspected the current local implementation, authored scene/prefabs, and UnitySetup.md before editing. No applicable AGENTS.md was present in the workspace or its ancestor directories. This folder remains outside Git; focused originals and SHA-256 hashes were saved before edits in **`Logs/Codex/CoinPatterns-Before/`**. Existing scene, prefab, artwork, animation, input, scoring, player, world/dash integration, health, and persistence code is retained. No game build or publishing was performed.

Coin opportunities now select Single / Row / Arc / RepeatedJumps with relative weights **1 / 2 / 2 / 1**, using the existing seeded random generator. A row or arc keeps close internal separation; a repeated sequence reserves time for three independent grounded presses. Arc height/spacing derives from the standing collider center, actual jump velocity/gravity/fixed-step integration, and current normal stage speed. The spawner reserves from the full pattern's last edge, at the global maximum dash speed, including flight/landing/reaction space. Reservations survive early collection, misses, stage changes, and camera-boundary changes. The cap counts individual coins and never admits a partial pattern. An impossible pattern under a total cap below three is skipped so hazards still rotate. Each coin reuses the original prefab, guarded contact, 50-point reward, and despawn path. All six settings and Inspector instructions are documented in [UnitySetup.md](UnitySetup.md#coin-patterns-and-full-pattern-clearance).

Changed files (plus Unity-imported metadata for the four new C# files):

| File | Change |
| --- | --- |
| `Assets/_Project/Scripts/World/CoinPatterns.cs` (new) | Small immutable three-position layout and weighted selection; no component or Update loop. |
| `Assets/_Project/Scripts/World/SpawnController.cs` | Whole-pattern instantiation, capacity checks, full-width spatial reservation, and reset bookkeeping. |
| `Assets/_Project/Scripts/World/SpawnSpacing.cs` | Exposes the existing conservative maneuver allowance as an edge gap. |
| `Assets/_Project/Scripts/Configuration/RunnerSettings.cs` | Four weights, row spacing, arc duration fraction, and validation. |
| `Assets/Settings/DefaultRunnerSettings.asset` | Serializes those six defaults on the already-assigned asset. |
| `Assets/_Project/Tests/EditMode/CoinPatternTests.cs` (new) | Counts/positions, discrete jump geometry at both speeds and stage transitions, repeated-jump timing, seeded weights, invalid configuration. |
| `Assets/_Project/Tests/PlayMode/CoinPatternIntegrationTests.cs` (new) | 150-point row, missing/duplicate contacts, threshold crossing, full-tail hazard clearance, caps, despawn, reset/menu/disable, seeded rotation. |
| `Assets/_Project/Tests/PlayMode/CoinPatternSceneTests.cs` (new) | Ten automated playtests loading the actual Game scene, its capsule/prefab, real input and physics. Uses a temporary settings clone and in-memory high scores; stops the session before unloading and never saves scene/assets. |
| `Assets/_Project/Tests/PlayMode/RunnerIntegrationTests.cs` | Makes the existing fixture partial so the focused pattern tests reuse its setup; existing test bodies/assertions are unchanged. |
| `Docs/UnitySetup.md` | Settings, formulas, Inspector verification, pattern behavior, and playtest instructions. |
| `Docs/RequirementCoverage.md` | This verification record. |

Executed against an isolated copy of the current **Assets, Packages, and ProjectSettings** in **`Logs/Codex/CoinPatternsValidation/`**, with Unity **6000.6.3f1**. Edit Mode used batch/no-graphics; Play Mode used batch with graphics enabled. Source/metadata parity was verified for all runtime/tests plus active settings, scenes, coin prefab, and package files (**87 files, zero mismatches**). The **14/14** backed-up scene/prefab/package/physics/timestep files retain their original hashes; all existing script/test metadata also matches. Final preservation details are in `Logs/Codex/CoinPatterns-Preservation.json`. The earlier score/difficulty results below are historical.

| Check | Actual result / local evidence |
| --- | --- |
| C# compilation | Passed; final logs contain no C# compiler warnings/errors or gameplay exceptions/assertions. |
| Edit Mode | **30 passed, 0 failed, 0 skipped**, 0.145642 s; `Logs/Codex/CoinPatterns-EditMode-Final.xml` and `.log`. |
| Play Mode | **36 passed, 0 failed, 0 skipped**, 56.8856715 s; `Logs/Codex/CoinPatterns-PlayMode-Final.xml` and `.log`. |
| Production-scene automated playtests | **10 passed**: ground row adds 150 item points without jumping; arc collects with one grounded jump at 6 and 7.5; arc survives a stage change during approach or on its first coin; repeated sequence collects with three released/repressed grounded jumps at both stages and across the threshold; held Space jumps/collects only once; an actual speed-15 arc dash leaves a grounded reaction window and permits clearing the next ground hazard with all three lives. |
| Existing regressions | Original 17 Edit Mode and 19 Play Mode cases remain, including guarded interactions, partial dash integration in both stages, shared movement distance/parallax/score, pause, restart, health, animations, input, and high-score persistence. |
| Preservation / scope | Existing Game/SampleScene, all three prefabs, Packages, physics/timestep settings unchanged. New `.meta` files copied from actual Unity import. No new package, manager, per-coin Update, runtime scoring shortcut, scene reconstruction, or build. |

The first sandbox launch stalled on local licensing; a licensed retry ran normally. The initial mathematical run exposed a continuous/discrete jump-height discrepancy; placement now accounts for Unity's integration. The first scene run reached all gameplay assertions but failed during fixture unload; the fixture now stops the session while its player exists. Final passing results above include these corrections without suppressing exceptions or weakening existing tests.

To reproduce, use the PowerShell invocation pattern below with project path `Logs/Codex/CoinPatternsValidation`, result paths `Logs/Codex/CoinPatterns-EditMode-Final.xml` / `CoinPatterns-PlayMode-Final.xml`, and matching `.log` paths, or run both complete suites in the current project's Test Runner. Omit `-nographics` for the graphics-enabled Play Mode run. Do not use `-quit` with the Test Runner.

**Remaining manual verification:** hands-on visual/feel acceptance in Game.unity at 16:9 and 4:3: recognizable row/arc spacing, comfortable one-jump timing, release/repress comfort in repeated sequences, dash before/during/after patterns near both hazard types, pause mid-pattern, missing coins, and repeated restart/menu transitions. Automated production physics playtests establish collection/clearance in tested cases; they do not replace human judgement of readability, input feel, or rendered UI/animation/parallax. No standalone build or cross-launch standalone persistence check was run for this update.

### Score/difficulty update, 2026-10-26

Current local scene and Inspector work is preserved. `GameSession` in `Assets/Scenes/Game.unity` references `Assets/Settings/DefaultRunnerSettings.asset`; its serialized Coin Points is now 50, with difficulty threshold 500 and harder multiplier 1.25. No scene, prefab, animation, UI, input, or package changes are part of this update. There is no Git repository in this folder, so a Git checkpoint could not be made. Focused originals and preservation hashes are retained locally in ignored `Logs/Codex/ScoreDifficulty-Before/`.

New coverage checks the imported active asset and its scene reference; 499/500 total-score boundary; a 50-point coin crossing the threshold; repeated higher scores retaining a single 1.25 multiplier; dash speeds 12/15; partial-dash distance and cooldown overshoot in both stages; pause/restart/menu/new-run reset; a stored high score of 9000 starting at normal speed; early-spawn reaction distance using the hardest dash; and the same distance reaching obstacles, parallax, and score. Existing collection, damage, input, UI binding, animation, and parallax tests remain in the suites.

Validation used an isolated copy of the current **Assets, Packages, and ProjectSettings** at `Logs/Codex/ScoreDifficultyValidation/`, opened with Unity **6000.6.3f1**. It never restored the old ZIP or saved/regenerated the production scene. The initial sandbox launch could not connect to local licensing; the retry with local licensing access completed both suites. The older results below are historical baseline evidence, not results for this change.

| Check | Executed result / local evidence |
| --- | --- |
| Unity import and C# compilation | Passed; no C# compiler warnings or errors in completed logs. |
| Edit Mode | **17 passed, 0 failed, 0 skipped**, 0.1362082 s; `Logs/Codex/ScoreDifficulty-EditMode.xml`, `Logs/Codex/ScoreDifficulty-EditMode-Final.log`. Includes importing the active settings asset and checking the preserved scene's reference in a preview scene. |
| Play Mode | **19 passed, 0 failed, 0 skipped**, 0.8795427 s; `Logs/Codex/ScoreDifficulty-PlayMode.xml`, `Logs/Codex/ScoreDifficulty-PlayMode.log`. |
| Source parity | All authored scripts, test assemblies, settings, and input assets checked against the validation copy match. |
| Preservation | **349/349** recorded hashes match: scenes, prefabs, animations/controllers, metadata, Packages, and ProjectSettings. Only the intended settings asset values were edited. |
| Focused diff checks | No whitespace errors; one movement call site consumes the dash multiplier/timers; runtime persistence contains no deletion calls. |
| Production visual playtest and standalone build/persistence | **Pending manual verification**; disposable fixture tests do not validate the rendered UI, actual art, jump feel, seams, or cross-launch standalone persistence. |

To reproduce, use the PowerShell pattern below with project path `Logs/Codex/ScoreDifficultyValidation`, result paths `Logs/Codex/ScoreDifficulty-EditMode.xml` / `ScoreDifficulty-PlayMode.xml`, and the corresponding log paths above, or run both suites in the current project's Test Runner. No package changes or temporary validation project are required for ordinary development.

### Original implementation validation

The real Unity Editor is installed and licensed. The user already had the production project open, so validation uses an isolated **Temp/CodexValidation** copy of Assets, Packages, and ProjectSettings. Unity imports and compiles that copy normally. This avoids closing the user's Editor, opening a second instance on the same project, or changing their current scene.

No production scene/prefab YAML or Animator asset is generated. Play Mode fixtures create disposable objects and Animator data through Unity APIs, then destroy them. Their inactive Canvas permits model/HUD binding without font/art imports, so these tests do not validate rendered UI. Integration scoring uses an in-memory store; PlayerPrefs tests use a unique isolated key and delete only that key. Input tests use the installed Unity.InputSystem.TestFramework InputTestFixture, restoring Input System state. No runtime test cheats or extra package dependencies are introduced.

PowerShell invocation pattern (paths shown for this workspace; change them for another machine):

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe' `
  -batchmode -nographics `
  -projectPath 'C:/Users/ToosArax/DinoRunner/Temp/CodexValidation' `
  -runTests -testPlatform EditMode `
  -testResults 'C:/Users/ToosArax/DinoRunner/Logs/Codex/EditMode-Final.xml' `
  -logFile 'C:/Users/ToosArax/DinoRunner/Logs/Codex/EditMode-Final.log'

# Run sequentially after the first Unity process has exited:
& 'C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe' `
  -batchmode -nographics `
  -projectPath 'C:/Users/ToosArax/DinoRunner/Temp/CodexValidation' `
  -runTests -testPlatform PlayMode `
  -testResults 'C:/Users/ToosArax/DinoRunner/Logs/Codex/PlayMode-Final.xml' `
  -logFile 'C:/Users/ToosArax/DinoRunner/Logs/Codex/PlayMode-Final.log'
```

Do not add `-quit` to these Test Runner commands; the runner exits on completion. Unity's Windows launcher may return before the test process finishes, so read the NUnit XML result and completed-run log, rather than interpreting the shell's immediate exit code as a passing test result. The ignored Logs/Codex directory retains local execution evidence. From a fresh checkout, run the suites in the configured project using the Test Runner; the temporary copy is not a required production dependency.

Final executed results:

| Check | Actual result / local evidence |
| --- | --- |
| Unity import and C# compilation | Completed in 6000.6.3f1; no C# compiler warnings or errors in final logs. |
| Edit Mode | **11 passed, 0 failed**, NUnit duration 0.0405404 s; `Logs/Codex/EditMode-Final.xml`, `Logs/Codex/EditMode-Final.log`. |
| Play Mode | **11 passed, 0 failed**, NUnit duration 0.7563561 s; `Logs/Codex/PlayMode-Final.xml`, `Logs/Codex/PlayMode-Final.log`. |
| Runtime source audit | No matches for legacy Input APIs, StandaloneInputModule, onClick, UnityEditor, TODO, NotImplementedException, or DeleteAll in Assets/_Project/Scripts. |
| Input JSON | Parsed successfully; Gameplay/System/UI maps contain the specified actions; IDs unique. Unity imported the asset and tests exercised its bindings. |
| Ignore baseline | Exact Appendix A content retained, plus the relevant `*.slnx` generated-solution rule. |
| Existing assets/configuration | Scene files, manifest, lockfile, Editor version, Player settings and build-scene settings match the pre-validation snapshot; no production YAML was edited. |
| Source parity / metadata | Final authored asset contents match the tested copy. New metadata comes from actual Unity import, copied only where missing; no guessed GUIDs. |
| Production build / visual acceptance | **Not run**; user scene/Inspector/art setup remains pending. |

The initial Edit Mode run passed the 10 model tests but failed four input tests because Edit Mode did not process their simulated player actions. Those cases were moved to supported Play Mode with the installed isolated InputTestFixture. A subsequent Play Mode run passed all 10 tests before the final parallax/freeze/cap additions. Final results above include those additions and supersede the initial attempts. Package resolution reused the existing exact versions without changing manifest or lockfile.

## Manual acceptance still required

- [ ] Complete all scene/Inspector/asset/layer/Animator/button mapping in [UnitySetup.md](UnitySetup.md).
- [ ] No missing references or Console errors when the configured Game scene starts; Main Menu shows Play/Quit/control hints.
- [ ] Play begins immediately with score zero and health three; live score/high-score stay visible throughout play and at GameOver.
- [ ] Actual Space jump, Shift dash, cooldown, airborne behavior, and ground detection feel correct at different frame rates.
- [ ] Ground hazards are jumpable; aerial hazards intersect jumps while safely clearing a grounded player; coins are reachable.
- [ ] All four actual Animator states and distinguishable clips work, including hit during jump and immediate fatal Death priority.
- [ ] Flash is brief, dash feedback visible, and both restore correctly after pause/death/restart/menu cycles.
- [ ] Escape and UI Pause/Resume work once per press; UI remains responsive at timeScale zero; holding Space/Shift across menus cannot activate a move.
- [ ] Death freezes player/hazards/spawning/score while its animation finishes; repeated Restart/Main Menu cycles restore everything.
- [ ] Two visible parallax layers wrap without seams at 16:9 and 4:3 and while dashing.
- [ ] Play several minutes to verify safe layouts and bounded live objects in the actual scene.
- [ ] A complete standalone build starts from Game.unity, quits correctly, and retains high score after closing/reopening.
- [ ] Git repository contains source, docs, Packages/lockfile, ProjectSettings, and Unity-generated metadata, with generated data/builds ignored.
- [ ] Complete student identity/repository URL, screenshots, short report/source acknowledgment, full runnable build, and required ZIP contents.

The source was implemented with Codex assistance; see README.md for sources and the student-review responsibility. No unaided-authorship claim, finished build, manual acceptance result, or submission is implied.
