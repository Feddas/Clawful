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
    private BlobTile whiteTile;

    [SerializeField]
    private BlobTile redTile;

    private Vector2 cellCornerToCenter;
    private Dictionary<Vector2Int, BlobBall> blobRigidBodies = new Dictionary<Vector2Int, BlobBall>();

    private Tilemap tilemap
    {
        get
        {
            if (_tilemap == null)
            {
                initTilemap();
            }
            return _tilemap;
        }
    }
    private Tilemap _tilemap;
    private void initTilemap()
    {
        _tilemap = GetComponent<Tilemap>();
        scoreBlob = new ScoreBlob(_tilemap);
        scoreBlob.OnRequestDestroyCell += DestroyCell;
        cellCornerToCenter = (Vector2)(tilemap.layoutGrid.cellSize / 2);
    }

    private void OnDestroy()
    {
        scoreBlob.OnRequestDestroyCell -= DestroyCell;
    }

    private void DestroyCell(Vector2Int position)
    {
        RemoveTile(position);

        if (blobRigidBodies.ContainsKey(position))
        {
            Destroy(blobRigidBodies[position].gameObject);
            blobRigidBodies.Remove(position);
        }
    }

    private void RemoveTile(Vector2Int position)
    {
        tilemap.SetTile((Vector3Int)position, null);
        overlay.SetTile((Vector3Int)position, null);
    }

    /// <summary> tilemap aligned to the same transform as the blobtiles tilemap. overlays are used to add number overlays to blob tiles. </summary>
    [SerializeField]
    private Tilemap overlay;
    //{
    //    get
    //    {
    //        if (_overlay == null)
    //        {
    //            _overlay = GetComponentInChildren<Tilemap>();
    //            Debug.Log("overlay set to be " + _overlay.name);
    //        }

    //        return _overlay;
    //    }
    //}
    //private Tilemap _overlay;

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

    /// <summary> This function is called by UnityEvent on TopBucketGate </summary>
    //public void AddToGrid(DropBall dropBall)
    //{
    //    var newPosition = SnapToClosestCell(dropBall.transform.position);
    //    newPosition += cellCornerToCenter; // account for cell pivot in lower left and ball pivot in center
    //    dropBall.LerpTo(newPosition);

    //    var floor = dropBall.GetComponentInChildren<BallFloorEvent>();
    //    if (floor != null)
    //    {
    //        floor.GridName = this.name;
    //        floor.OnHitFloor += Floor_OnHitFloor;  // TODO: -= when ball is cleared from scoring
    //    }
    //}

    public void AddToGrid(BlobBall blobBall)
    {
        var newPosition = SnapToClosestCell(blobBall.transform.position);
        newPosition += cellCornerToCenter; // account for cell pivot in lower left and ball pivot in center
        blobBall.LerpTo(newPosition);

        var floor = blobBall.GetComponentInChildren<BallFloorEvent>();
        if (floor != null)
        {
            floor.GridName = this.name;
            floor.OnHitFloor += Floor_OnHitFloor;  // TODO: -= when ball is cleared from scoring
            floor.OnFloorMissing += Floor_OnFloorMissing;
        }
    }

    private void Floor_OnFloorMissing(BlobBall blobBall)
    {
        var gridPosition = tilemap.WorldToCell(blobBall.transform.position);
        RemoveTile((Vector2Int)gridPosition);
        blobBall.SetState(isATile: false);
    }

    private void Floor_OnHitFloor(BlobBall dropBall)
    {
        var targetTile = (Vector2Int)tilemap.WorldToCell(dropBall.transform.position);
        var afterlife = dropBall.PowerActivation(targetTile, scoreBlob);

        switch (afterlife)
        {
            default:
            case BlobBall.AfterUsed.Destroy:
                Destroy(dropBall.gameObject);
                break;
            case BlobBall.AfterUsed.AddToGrid:
                // deactivate no-tile based ball
                dropBall.SetState(isATile: true);
                blobRigidBodies[targetTile] = dropBall;

                // attach to tilemap
                if (dropBall.Color == whiteTile.Color)
                {
                    Set(whiteTile, targetTile, (int)dropBall.PointValue);
                }
                else if (dropBall.Color == redTile.Color)
                {
                    Set(redTile, targetTile, (int)dropBall.PointValue);
                }
                break;
        }
    }

    /// <returns> the world position center of the tile closest to the given<paramref name="worldPosition"/></returns>
    private Vector2 SnapToClosestCell(Vector2 worldPosition)
    {
        var gridPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.CellToWorld(gridPosition);
    }

    private ScoreBlob scoreBlob;

    public void Set(TileBase coloredTile, Vector2Int position, int overlayIndex = -1)
    {
        // show and store point value
        if (overlayIndex != -1) // add overlay tile
        {
            overlay.SetTile((Vector3Int)position, overlayTiles[overlayIndex]);
            scoreBlob[position] = (byte)overlayIndex;
        }

        // link cell with neighboring cells
        this.tilemap.SetTile((Vector3Int)position, coloredTile);
    }

    private void Start()
    {
        initTilemap();

        // Test rule tiles render
        Set(whiteTile, new Vector2Int(0, 0), 0);
        Set(whiteTile, new Vector2Int(1, 1));
        Set(whiteTile, new Vector2Int(1, 0), 10);
        Set(redTile, new Vector2Int(0, 1), 1);
        Set(redTile, new Vector2Int(0, 2), 9);
        Set(redTile, new Vector2Int(0, 3), 4);
        Set(whiteTile, new Vector2Int(0, 4), 1);
        Set(whiteTile, new Vector2Int(0, 5), 9);
        Set(whiteTile, new Vector2Int(0, 6), 4);
        Set(whiteTile, new Vector2Int(0, 7), 4);

        Set(redTile, new Vector2Int(2, 1), 8);
        Set(redTile, new Vector2Int(2, 0), 9);
        Set(redTile, new Vector2Int(3, 0), 6);
        Set(redTile, new Vector2Int(4, 0), 4);
        Set(whiteTile, new Vector2Int(5, 0));
        Set(whiteTile, new Vector2Int(6, 0));
        Set(whiteTile, new Vector2Int(7, 0));
        Set(whiteTile, new Vector2Int(7, 1));

        //Debug.Log("tile1 value " + tileValues[new Vector2Int(0, 1)]);
        //Debug.Log("tile2 value " + tileValues[new Vector2Int(0, 2)]);
        //Debug.Log("tile3 value " + tileValues[new Vector2Int(0, 3)]);
    }
}
