# Windows standalone acceptance — pending

The release was built on Linux. These checks require a Windows x86_64 machine; they have not been reported as passing.

1. Extract the complete submission ZIP. Keep every file and folder inside `Build` together. Launch `Build/DinoRunner.exe`.
2. On the main menu, use arrows/Enter and the mouse. Select Play. Confirm three lives, current score, and high score in the HUD.
3. Press/release Space for single jumps and either Shift for dash. Confirm dash ends and has a cooldown. Escape must pause/resume; the pause buttons must also work. Focus loss should pause the run until Resume.
4. Play through naturally spawned ground rubble and aerial drones. Let a ground obstacle hit, jump into an aerial obstacle, and confirm the health bar loses a life per accepted encounter with visible red hit feedback. A drone should be safe while standing. Decorative rubble/rebar and rotor overhangs are intentional and extend outside the hitboxes.
5. Collect uranium. Each item adds 50 points in addition to distance scoring. Check a ground row, an arc cleared with one jump, and repeated-jump items requiring separate presses.
6. Reach 500 current-run points. Movement becomes 25% faster once; confirm it does not accelerate again at 1000. Check dash, background parallax, and asphalt motion at both stages.
7. Lose the third life. Confirm Death animation, the GAME OVER panel, final score, and frozen world. Restart must restore three lives, score 0, and the base speed. Return to Main Menu and Quit; verify the player process closes.
8. Repeat layout/readability checks using these PowerShell launches from the extracted ZIP folder, closing the game between runs:

   ```powershell
   .\Build\DinoRunner.exe -screen-fullscreen 0 -screen-width 1280 -screen-height 720
   .\Build\DinoRunner.exe -screen-fullscreen 0 -screen-width 960 -screen-height 720
   .\Build\DinoRunner.exe -screen-fullscreen 0 -screen-width 800 -screen-height 450
   ```

   Check main menu, HUD, pause, Game Over, and full scenery/asphalt coverage at each size.
9. Set a new high score and record its value. Return to Main Menu, Quit fully, and launch again. Select Play and confirm the same high score appears in the HUD, while the new current score starts fresh. Company/product identifiers must remain `DefaultCompany` / `DinoRunner`.
10. Open the repository URL from Repository.txt in a signed-out browser, or clone it without a privileged account, and compare its main branch with the recorded commit. Review the report, AI assistance acknowledgment, and the provenance of the student-supplied scenery/props before uploading.

Record actual observations and any failures; do not treat the automated Editor results as standalone playtesting.
