# Grids

Grids are where the claw throws balls. In the Grid, balls can form into groups of the same color.

Grids use UnityEngine.RuleTile `BlobTile.cs` to provide:
- visual joining of same tile types
- visual overlay of tile value

`BlobTile.cs`'s are managed by `BlobGrid.cs`:
- stores tile point value in Dictionary tileValues

Tile sprites have an overlay tilemap. Its used to display text for each ball. Overlays were not put on the same tilemap due to rendered bugs using the sorting axis provided by [mode Individual](docs.unity3d.com/2022.3/Documentation/Manual/class-TilemapRenderer.html).

If Tile's lose their floor, they convert back into a rigidbody.

## Tiles

To add a new tile color go to "Create/2D/Clawful Tile". It should be setup smiliar to TileBlobRed.asset. Afterwards, any BallSpawner has to have its color list include the exact same color.

# Balls

Balls are initiated by `BallSpawner.cs`. The player uses the claw to help transition balls to `EnterGrid.cs`, which gates when balls will be assigned to a players grid. Once in a grid, balls can be scored with `BombBall.cs`. BombBalls inherit BlobBalls.

## back and forth
Balls are either in a rigid body state or a tilemap state.
- rigidbody - the ball interacts with physics. The rigidbody gameobject exists, but is deactivated, while the ball is a tilemap.
- tilemap - the ball visually joins adjacent (vertially and horizontally) balls of the same color.

To a tilemap - Balls enter the grid as rigidbodies. They remain rigidbodies until their Y-position is below their `FallsUntil + .1` value. While a rigidbody, they have an active trigger collider below them. When this trigger enters a tilemap tile, or the edge of the grid, the ball is converted to a tilemap and FallsUntil is updated only to the rigidbodies current y-value only if FallsUntil's value is set to float.maxvalue (To ensure it doesn't override the value set by tile deleted event).

To a rigidbody - when any tile is deleted, all tiles recieve the `tile removed` event containing an x,y payload. if this tile is in the same x-column and above the y-row, it's converted to a rigidbody (if it isn't already) and `FallsUntil` is changed by -1.

`FallsUntil` - This value represents when the ball is allowed to check if it should transition from a rigidbody into a tilemap tile. This value is modified when a tile below it is removed from the grid.

# Dev Ops

CI (Continous Integration) definitions are in [.github/workflows](../../.github/workflows). These 
files tell GitHub to create builds ([as artifacts to the bottom of 
actions](https://github.com/Feddas/Clawful/actions/runs/10231069973)) and to deploy the WebGL build 
to GitHub pages. For details, [this link explains how CI was 
enabled](https://game.ci/docs/github/activation/#personal-license).

- [everypush.yml](../../.github/workflows/everypush.yml) - Builds Win, Linux, & WebGL every push to the repo. If pushing to main, also deploys WebGL website.
- [manual.yml](../../.github/workflows/manual.yml) - Runs similiar flow as everypush,yml. But any branch. Probably (not tested) uses whatever is the last commit to deploy WebGL website.

# Input System

https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputAction.html

## Fix Editor

The project is using inputsystem v1.7.0, which has a bug when using the listening for key feature. Fix in file C:\Users\[username]\AppData\Local\Unity\cache\packages\packages.unity.com\com.unity.inputsystem@1.7.0\InputSystem\Editor\Internal\AdvancedDropdown\AdvancedDropdownGUI.cs
Line 16 add an 'r' to "ToolbarSeachTextField" => "ToolbarSearchTextField" as shown in https://youtu.be/lclDl-NGUMg?t=318

## UI

UiSelectedOnEnable.cs - UI navigation (without a mouse pointer) needs a starting point. This script defines what that starting point is, `FirstSelected`. Therefore, this script should be on every full screen UI panel.

## Misc tips

PlayerInput properties can be modified with stackoverflow.com/questions/61611367

