ESCAPE.EXE - STARTER SCRIPTS
============================
Drag this whole folder into Unity's Assets/Scripts folder.
Unity 6, Universal 2D template, Input System (default in Unity 6).

PIXEL ART SETUP (decided 13 Nov)
  Every sprite: Pixels Per Unit 16, Filter Mode Point (no filter), Compression None
  Main Camera: Orthographic + Pixel Perfect Camera (Assets PPU 16, Reference Resolution 480 x 270)
  1 tile = 16 x 16 pixels = 1 unit. Pip = 16 x 16 = 1 unit.
  Monitor frame image = 480 x 270 with the screen cut out. Screen (play area) = 416 x 224 px = 26 x 14 units.
These scripts were written carefully but NOT tested in Unity. Expect a small fix or two.

SCENE NAMES (must match exactly, and all must be added to File > Build Profiles > Scene List)
  Boot, Level1_Game, Level2_Desktop, Level3_Through, Level4_Delete, Level5_Notepad,
  Ending_Return, Ending_Freedom, Ending_Creator

LAYERS: Player, Ground, TitleBar, Files, DropZone      TAG: Player

WHICH SCRIPT GOES WHERE
-----------------------
EVERY LEVEL
  PlayerController  -> Pip (Rigidbody2D, BoxCollider2D, child "GroundCheck"; Ground Layer = Ground + TitleBar)
  Hazard            -> spikes, and a big invisible KillZone trigger under the level
  LevelExit         -> exit triggers (type the next scene name)
  ShowOnTrigger     -> tooltips, pop-ups, anything that should appear when Pip walks somewhere

BOOT
  BootScreen        -> Managers object (Space = start)
  LevelSelect       -> its own object, Boot scene ONLY (keys 0-5, R restart, 9 = all secrets)

LEVEL 1 (inside the game)
  Coin              -> each coin (CircleCollider2D, Is Trigger)
  CoinCounter       -> the "Coins: 0" TextMeshPro text on the Canvas
  ShowOnTrigger     -> zone at the TODO sign: shows "?" bubble + "Build failed" pop-up,
                       a 2nd ShowOnTrigger on the same zone with delay 1.5 shows the crack
  GlitchExit        -> the crack (trigger). Needs a full-screen white UI Image (alpha 0, Raycast Target OFF)

LEVEL 2 (desktop)
  CameraZoom        -> Main Camera: the zoom-OUT reveal (Start Size 3 on Pip -> End Size 8.4375 whole view; switches Pixel Perfect off during the zoom)
  DraggableWindow   -> root of the sticky note and calculator windows (scale stays 1,1,1)
  DragManager       -> Managers object (Draggable Layers = TitleBar + Files, Player Layer = Player)
  SecretFile        -> pip_sketch_2014.png (secretId = secret1)
  LevelExit         -> zone in front of the game window's left crack -> Level3_Through

LEVEL 3 (through the frozen game window)
  DraggableWindow   -> the game window, with "Can Drag" UNTICKED (it's Not Responding)
  DroppableFile     -> spring.png (Spawn In Game = the hidden spring), joke files (Joke Popup only)
  DropZone          -> trigger BoxCollider2D over the window's play area, layer DropZone
  SecretFile        -> old_game.zip (secretId = secret2)
  LevelExit         -> right-hand crack -> Level4_Delete

LEVEL 4 (delete?)
  RedCursor + Hazard -> the red cursor (trigger collider)
  DeletePopup       -> Managers object; NO button -> ClickNo, YES button -> ClickYes
  SecretFile        -> dont_open.txt (secretId = secret3)
  LevelExit         -> the Notepad icon -> Level5_Notepad

LEVEL 5 (notepad)
  CameraZoom        -> Main Camera (optional): slow zoom-IN on the Notepad window
  NotepadEnding     -> Managers object (Show When Done = exits parent, Secret Choice = gold ending.png)
  LevelExit         -> left exit -> Ending_Return, right exit -> Ending_Freedom,
                       gold file -> Ending_Creator

ENDING SCENES
  NotepadEnding     -> types Sam's last line, then shows the "ENDING X / 3" card
  SceneLoader       -> "Play again" button -> PlayAgain

COMMON GOTCHAS
  - "Keyboard/Mouse not found" error: install Input System in Window > Package Manager.
  - Buttons don't click: the EventSystem needs "Input System UI Input Module" (Unity offers to replace it - click yes).
  - Pip stretches on a window: the window root's scale isn't 1,1,1.
  - LoadScene fails: the scene isn't in Build Profiles > Scene List, or the name is spelled differently.
  - Pip stands on thin air: a trigger is on the Ground or TitleBar layer. Keep triggers on Default or DropZone.
