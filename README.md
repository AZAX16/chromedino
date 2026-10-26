# Chrome Dino — Unity C# implementation

Target Editor: **6000.6.3f1 (45d8eee7de74)**, from ProjectSettings/ProjectVersion.txt. This is the existing Unity 6.6 URP 2D project. The repository includes the C# systems, input asset, current authored Game scene, prefabs, Animator assets, UI, and settings. Remaining manual acceptance checks are documented below.

Follow [Docs/UnitySetup.md](Docs/UnitySetup.md) for every component, field, action reference, layer, animation transition, and button callback. [Docs/RequirementCoverage.md](Docs/RequirementCoverage.md) distinguishes source coverage, executed checks, and pending acceptance work. The original [implementation specification](Docs/Chrome_Dino_CSharp_Codex_Spec.md) remains the requirements source.

## Continue on another machine

```sh
git clone https://github.com/AZAX16/chromedino.git
cd chromedino
```

Add the cloned folder in Unity Hub and open it with **6000.6.3f1**. Let Unity resolve the committed `Packages/manifest.json` and `Packages/packages-lock.json` and import the assets. Open **`Assets/Scenes/Game.unity`** to continue working on the current game. Unity regenerates `Library`, IDE project files, and other ignored local caches.

Run both Edit Mode and Play Mode suites in **Window > General > Test Runner**. See [Docs/GitHubVerification.md](Docs/GitHubVerification.md) for the verification performed before the initial push. The existing build-scene list still points to `SampleScene.unity`; select `Game.unity` first when preparing a standalone build.

## Running

1. Open with the exact recorded Editor and let Unity resolve existing packages/import assets.
2. Follow UnitySetup, starting with the RunnerSettings asset, Player/ground, and GameSystems references. Wire button On Click events manually to GameSession; code never adds duplicate button listeners.
3. Configure RunnerControls and the one InputSystemUIInputModule. Space jumps, Shift dashes, Escape pauses/resumes; arrows/Enter navigate menus.
4. Run the Unity Test Runner suites, then validate actual art, animations, menus, layout, spawn fairness, and persistence in the configured scene.
5. Make Game.unity the first enabled scene in Build Profiles. Build and reopen a standalone player to verify persistent high score.

## Code structure and gameplay flow

All runtime code is under Assets/_Project/Scripts, in namespace ChromeDino and assembly ChromeDino.Runtime. GameSession owns state and timeScale and initializes dependencies explicitly. RunnerInputReader converts InputAction callbacks to queued requests; the session handles Pause before gameplay requests. PlayerController executes eligible single jumps in physics steps and controls dash/cooldown; PlayerHealth alone changes lives.

WorldScrollController integrates forward distance once, including partial dash steps. SpawnController moves a bounded list of interactables and uses maximum-speed spacing before accepting a randomized interval. Parallax layers consume the same step distance. GroundObstacle and AerialObstacle inherit Obstacle, which inherits the required abstract Interactable base; CoinCollectible inherits Interactable directly. The player dispatches through that base class without concrete-type checks. Sweeps and trigger callbacks share the once-per-encounter guard. Coins add points once and immediately deactivate/despawn.

ScoreTracker combines floored accumulated distance points with item points, saturates at int.MaxValue, and updates the live record. PlayerPrefsHighScoreStore writes the namespaced key ChromeDino.HighScore.v1 only at dirty-record checkpoints. HUD and menus observe model/state events. Hit/death animation and tint display decisions do not determine damage validity. Restart cleans up the existing scene explicitly rather than reloading it.

## Dependencies

These exact direct dependencies are preserved from Packages/manifest.json. There are no added packages, standalone .NET solutions, downloaded DLLs, or fabricated lockfile entries. UPM owns resolution. Test assemblies reference the installed Unity.InputSystem.TestFramework helper; TMP is included in uGUI.

| Dependency | Version | Purpose / setup |
| --- | --- | --- |
| Unity Editor | 6000.6.3f1 | Use this installed Editor; no migration. |
| com.unity.2d.animation | 16.0.1 | Existing 2D toolchain; preserved. |
| com.unity.2d.aseprite | 6.0.0 | Existing 2D toolchain; preserved. |
| com.unity.2d.psdimporter | 15.0.1 | Existing 2D toolchain; preserved. |
| com.unity.2d.sprite | 1.0.0 | Existing 2D toolchain; preserved. |
| com.unity.2d.spriteshape | 16.0.0 | Existing 2D toolchain; preserved. |
| com.unity.2d.tilemap | 1.0.0 | Existing 2D toolchain; preserved. |
| com.unity.2d.tilemap.extras | 9.0.1 | Existing 2D toolchain; preserved. |
| com.unity.2d.tooling | 4.0.1 | Existing 2D toolchain; preserved. |
| com.unity.collab-proxy | 2.13.6 | Existing version-control integration; preserved. |
| com.unity.ide.rider | 3.0.38 | Existing Rider integration; preserved. |
| com.unity.ide.visualstudio | 2.0.28 | Existing Visual Studio integration. |
| com.unity.inputsystem | 1.20.1 | Gameplay/System action references and UI event module; configure RunnerControls. |
| com.unity.learn.iet-framework | 6.0.7 | Existing template/tutorial support; preserved. |
| com.unity.render-pipelines.universal | 17.6.0 | Existing URP 2D renderer; preserve renderer assets. |
| com.unity.test-framework | 1.8.0 | Edit Mode and Play Mode tests; Window > General > Test Runner. |
| com.unity.timeline | 6.6.0 | Existing timeline support; preserved. |
| com.unity.ugui | 2.6.0 | Canvas, Slider, menus, Buttons, bundled TMP; import TMP Essential Resources in Unity. |
| com.unity.visualscripting | 1.9.12 | Existing template package; runner does not require visual graphs. |
| com.unity.modules.accessibility | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.adaptiveperformance | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.ai | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.androidjni | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.animation | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.assetbundle | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.audio | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.cloth | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.director | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.imageconversion | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.imgui | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.jsonserialize | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.particlesystem | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.physics | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.physics2d | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.physicscore2d | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.screencapture | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.terrain | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.terrainphysics | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.tetgen | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.tilemap | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.timelinefoundation | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.ui | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.uielements | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.umbra | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unityanalytics | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unitywebrequest | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unitywebrequestassetbundle | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unitywebrequestaudio | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unitywebrequesttexture | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.unitywebrequestwww | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.vectorgraphics | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.vehicles | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.video | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.wind | 1.0.0 | Existing built-in Unity module; preserved. |
| com.unity.modules.xr | 1.0.0 | Existing built-in Unity module; preserved. |

## Validation and remaining work

See Docs/RequirementCoverage.md for exact commands, results, and limitations. Automated fixtures exercise source invariants and disposable integration objects; they cannot establish production scene/art quality. No runnable production build or final UI/Inspector configuration is claimed. Existing scene YAML and package manifest/lockfile remain unchanged.

The project is tracked on the `main` branch of [AZAX16/chromedino](https://github.com/AZAX16/chromedino). The supplied .gitignore baseline is copied verbatim from the spec, with an additional *.slnx rule for Unity 6 generated solutions. Unity-generated `.meta` files are tracked; Library, Temp, Logs, and builds are ignored. Existing ZIP archives and the pre-artwork checkpoint are retained alongside the current project files.

## Assistance, sources, and submission

Implementation was assisted by OpenAI Codex. The student must review, understand, and take responsibility for the submitted code, identify any human collaborators themselves, and complete the course's required report/screenshots/repository/build ZIP. No student identity or repository URL has been invented. The spec states 26 Mehr 1405 as the deadline and GD_Hw1_[firstName]_[lastName]_[StudentId].zip as the submission filename.

Technical references used:

- [Unity 6.6 Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html): the Unity 6 movement property used here.
- [Unity 6.6 Collider2D.Cast](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D.Cast.html): shape sweeps supplement trigger callbacks.
- Installed com.unity.inputsystem 1.20.1 source: InputAction, InputSystemUIInputModule, and Tests/TestFixture/InputTestFixture.cs. The test helper explicitly supports Play Mode, where the input tests live.
- Installed uGUI assembly definitions: UnityEngine.UI and Unity.TextMeshPro; installed Test Framework 1.8.0 runner.
- The supplied Docs/Chrome_Dino_CSharp_Codex_Spec.md, including Appendix A's .gitignore. The referenced homework PDF was not present in this workspace; the supplied spec was used independently as requested.
