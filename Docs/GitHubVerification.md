# GitHub handoff verification

Verified on **2026-10-27** (Asia/Tehran) for the initial `main` push to [AZAX16/chromedino](https://github.com/AZAX16/chromedino).

## Executed checks

Unity **6000.6.3f1 (45d8eee7de74)** ran the complete existing suites against an isolated copy of the current `Assets`, `Packages`, and `ProjectSettings`. All **541 files** in those directories matched the originals before testing. The originals retained their SHA-256 hashes after testing.

| Suite | Passed | Failed | Skipped | NUnit duration |
| --- | ---: | ---: | ---: | ---: |
| Edit Mode | 32 | 0 | 0 | 0.1581787 s |
| Play Mode | 37 | 0 | 0 | 41.8062784 s |

The Edit Mode run used batch mode with `-nographics`; Play Mode used batch mode with graphics enabled. Both completed successfully. The suites cover gameplay invariants, scoring and difficulty, parallax, input routing, health, dash, persistence, integration behavior, and coin-pattern playtests loading the actual `Assets/Scenes/Game.unity` scene. Every asset file has its `.meta` file, and no duplicate asset GUIDs were found.

Local execution evidence is retained in the ignored `Logs/Codex` directory:

- `GitHubPublish-EditMode.xml` and `GitHubPublish-EditMode-Licensed.log`
- `GitHubPublish-PlayMode.xml` and `GitHubPublish-PlayMode.log`
- `GitHubPublish-Before.json`, containing the original project-file hashes

The first sandboxed launch could not connect to the local Unity license service. The completed results above come from the subsequent licensed runs. No gameplay fixes were needed.

## Reproduce after cloning

Open the cloned project in Unity **6000.6.3f1**, allow package resolution and asset import, then run both complete suites from **Window > General > Test Runner**. Open `Assets/Scenes/Game.unity` for the current authored game. See the [README](../README.md#continue-on-another-machine) for cloning instructions.

For command-line runs, use this PowerShell pattern after closing the Editor for the target project, adjusting the paths for your machine:

```powershell
$unityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe'
$runnerProject = 'C:/path/to/chromedino'

& $unityEditor -batchmode -nographics -projectPath $runnerProject `
  -runTests -testPlatform EditMode `
  -testResults "$runnerProject/Logs/EditMode.xml" `
  -logFile "$runnerProject/Logs/EditMode.log"

# Wait for Unity to finish and verify the XML before starting the next suite.
& $unityEditor -batchmode -projectPath $runnerProject `
  -runTests -testPlatform PlayMode `
  -testResults "$runnerProject/Logs/PlayMode.xml" `
  -logFile "$runnerProject/Logs/PlayMode.log"
```

Do not add `-quit`; the Test Runner exits on completion. Read the completed NUnit XML and log to verify success, because the Windows launcher can return before the test process finishes.

## Handoff contents and remaining checks

The repository retains the current assets, scenes, prefabs, animations, scripts, tests, metadata, package manifest/lockfile, project settings, documentation, existing ZIP archives, and pre-artwork checkpoint. Unity caches, logs, temporary files, generated IDE projects, user settings, and builds follow the existing `.gitignore`.

No standalone build or hands-on visual acceptance was performed for this handoff. The existing build-scene list points to `SampleScene.unity`; make `Game.unity` the first enabled scene before preparing a standalone build. The manual checks in [RequirementCoverage.md](RequirementCoverage.md) still apply.
