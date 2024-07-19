using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Custom UnityEngine.RuleTile for blob cells to visually auto-join and to track their per cell overlay value. </summary>
[CreateAssetMenu(fileName = "BlobTile", menuName = "2D/Clawful Tile", order = 99)]
public class BlobTile : RuleTile
{
    [Tooltip("Color of all tiles created with this asset.")]
    public Color Color = Color.red;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.color = Color;
    }
}
