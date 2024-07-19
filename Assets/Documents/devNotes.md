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
