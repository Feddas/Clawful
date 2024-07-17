using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[RequireComponent(typeof(Tilemap))]
public class BlobGrid : MonoBehaviour
{
    //[SerializeField]
    //private Vector2Int gridSize = new Vector2Int(10, 10);

    [SerializeField]
    private BlobTile player1;

    [SerializeField]
    private BlobTile player2;

    private Tilemap tilemap
    {
        get
        {
            if (_tilemap == null)
            {
                _tilemap = GetComponent<Tilemap>();
            }
            return _tilemap;
        }
    }
    private Tilemap _tilemap;

    private Dictionary<Vector2Int, int> tileValues = new Dictionary<Vector2Int, int>();

    [Tooltip("all sprites (assigned to tiles) available to be overlays.")]
    public Tile[] overlayTiles;

    // Figuring out colliders should lead to joining colliders. joined colliders = finding all linked blob cells.

    /// <summary> tile neighbors are never outside these bounds </summary>
    //public RectInt Bounds
    //{
    //    get
    //    {
    //        Vector2Int position = new Vector2Int(-this.gridSize.x / 2, -this.gridSize.y / 2);
    //        return new RectInt(position, this.gridSize);
    //    }
    //}

    /// <returns> the world position center of the tile closest to the given <paramref name="worldPosition"/></returns>
    //public Vector2 SnapToClosestCell(Vector2 worldPosition)
    //{
    //    var gridPosition = tilemap.WorldToCell(worldPosition);
    //    return tilemap.CellToWorld(gridPosition);
    //}

    public void Set(TileBase player, Vector2Int position, int overlayIndex = -1)
    {
        // store overlay value
        tileValues[position] = overlayIndex;
        if (overlayIndex != -1) // add overlay tile
        {
            var overlay = (Vector3Int)position + Vector3Int.forward;
            this.tilemap.SetTile(overlay, overlayTiles[overlayIndex]);
        }

        // link cell with neighboring cells
        this.tilemap.SetTile((Vector3Int)position, player);
    }

    private void Start()
    {
        // Test rule tiles render
        Set(player1, new Vector2Int(0, 0), 0);
        Set(player1, new Vector2Int(1, 1));
        Set(player1, new Vector2Int(1, 0), 10);

        // Test rule tiles render
        Set(player2, new Vector2Int(0, 1), 1);
        Set(player2, new Vector2Int(0, 2), 9);
        Set(player2, new Vector2Int(0, 3), 4);

        Debug.Log("tile1 value " + tileValues[new Vector2Int(0, 1)]);
        Debug.Log("tile2 value " + tileValues[new Vector2Int(0, 2)]);
        Debug.Log("tile3 value " + tileValues[new Vector2Int(0, 3)]);
    }
}
