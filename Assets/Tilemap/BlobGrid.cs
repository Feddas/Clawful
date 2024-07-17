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
    private RuleTile blobTile;

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

    /// <summary> tilemap aligned to the same transform as the tilemap the blobtiles will belong to. overlays is used to add number overlays to blob tiles. </summary>

    private Tilemap overlays
    {
        get
        {
            if (_overlays == null)
            {
                _overlays = GameObject.Instantiate<Tilemap>(overlayPrefab, tilemap.transform.position, tilemap.transform.rotation);
                _overlays.transform.parent = tilemap.transform.parent;
            }

            return _overlays;
        }
    }
    private Tilemap _overlays;

    public Tilemap overlayPrefab;

    [Tooltip("all sprites (assigned to tiles) available to be overlays.")]
    public Tile[] overlayTiles;

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

    public void Set(Vector3Int position, int overlayIndex = -1)
    {
        this.tilemap.SetTile(position, blobTile);
        if (overlayIndex != -1)
        {
            overlays.SetTile(position, overlayTiles[overlayIndex]);
        }
    }

    private void Start()
    {
        // Test rule tiles render
        Set(new Vector3Int(0, 0, 0), 0);
        Set(new Vector3Int(1, 0, 0), 10);
        Set(new Vector3Int(1, 1, 0));
    }
}
