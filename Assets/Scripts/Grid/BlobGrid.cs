using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Manages the PuyoPuyo aspect of the game for a single player. </summary>
[RequireComponent(typeof(Tilemap))]
public class BlobGrid : MonoBehaviour
{
    /// <summary> Raised when any cell (tile & rigidbody) on this grid is removed. </summary>
    public event Action<Vector2> OnDestroyedCell;

    /// <summary> Balls can't be activated when above this height </summary>
    private const int gridHeight = 8;

    [SerializeField]
    private TextMeshProUGUI ScoreText;

    [Tooltip("tilemap aligned to the same transform as the blobtiles tilemap. overlays are used to add text over tiles in thie tilemap.")]
    [SerializeField]
    private Tilemap overlay;

    [Tooltip("all sprites (assigned to tiles) available to be overlays.")]
    [SerializeField]
    private Tile[] overlayTiles;

    [SerializeField]
    private BlobTile whiteTile;

    [SerializeField]
    private BlobTile redTile;

    private int CurrentScore;
    private Vector2 cellCornerToCenter;
    private Dictionary<Vector2Int, BlobBall> blobRigidBodies = new Dictionary<Vector2Int, BlobBall>();
    private ScoreBlob scoreBlob;

    /// <summary> Batch of all cells that were recently destroyed. This avoids cells trying the fall even when they're about to be destroyed. </summary>
    private List<Vector2Int> destroyedCells = new List<Vector2Int>();

    /// <summary> allows tilemap to be initialized before Start() </summary>
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

    /// <summary> Called by EnterGrid.cs's OnBlobEntered UnityEvent </summary>
    public void AddToGrid(BlobBall blobBall)
    {
        if (false == (blobBall is BombBall))
        {
            // subscribe to tile removed event that happens when bombball drops. This event controls the ball converting from a tile to a rigidbody
            this.OnDestroyedCell += blobBall.OtherBallRemoved;
        }

        var floor = blobBall.FloorSensor;
        floor.GridName = this.name;
        floor.MaxHeight = this.transform.position.y + gridHeight + 0.6f;
        floor.OnHitFloor += Floor_OnHitFloor;
        floor.OnFloorMissing += Floor_OnFloorMissing;
    }

    /// <returns> the world position center of the tile closest to the given<paramref name="worldPosition"/></returns>
    public Vector2 SnapToClosestCell(Vector2 worldPosition)
    {
        var gridLocal = tilemap.WorldToCell(worldPosition);
        var gridWorld = (Vector2)tilemap.CellToWorld(gridLocal);
        return gridWorld + cellCornerToCenter; // account for cell pivot in lower left and ball pivot in center;
    }

    private void initTilemap()
    {
        _tilemap = GetComponent<Tilemap>();
        scoreBlob = new ScoreBlob(_tilemap);
        scoreBlob.OnRequestDestroyCell += DestroyCell;
        scoreBlob.OnScored += AddScore;
        cellCornerToCenter = (Vector2)(tilemap.layoutGrid.cellSize / 2);
    }

    private void OnDestroy()
    {
        scoreBlob.OnRequestDestroyCell -= DestroyCell;
        scoreBlob.OnScored -= AddScore;
    }

    private void AddScore(int scoreDelta)
    {
        CurrentScore += scoreDelta;
        ScoreText.text = CurrentScore.ToString();
    }

    /// <summary> All cells are finished being destroyed from a bombball. notifiy the other cells it's time to fall down. </summary>
    private void DestroyedCellsCleanup()
    {
        foreach (var position in destroyedCells)
        {
            if (OnDestroyedCell != null)
            {
                var worldPosition = (Vector2)tilemap.CellToWorld((Vector3Int)position) + cellCornerToCenter;
                OnDestroyedCell.Invoke(worldPosition);
            }
        }
        destroyedCells.Clear();
    }

    private void DestroyCell(Vector2Int position)
    {
        RemoveRigidBody(position);
        RemoveTile(position);
        destroyedCells.Add(position);
    }

    private void RemoveRigidBody(Vector2Int position)
    {
        if (blobRigidBodies.ContainsKey(position)) // this if check is only needed for testing, since tiles made in Start() don't have rigidbodies
        {
            var blobBall = blobRigidBodies[position];
            this.OnDestroyedCell -= blobBall.OtherBallRemoved;
            blobBall.FloorSensor.OnHitFloor -= Floor_OnHitFloor;
            blobBall.FloorSensor.OnFloorMissing -= Floor_OnFloorMissing;
            Destroy(blobBall.gameObject);
        }
    }

    /// <summary> Removes <paramref name="position"/> from tilemap and from <seealso cref="blobRigidBodies"/>.</summary>
    private void RemoveTile(Vector2Int position)
    {
        tilemap.SetTile((Vector3Int)position, null);
        overlay.SetTile((Vector3Int)position, null);
        blobRigidBodies.Remove(position);
    }

    private void Floor_OnFloorMissing(BlobBall blobBall)
    {
        var gridPosition = (Vector2Int)tilemap.WorldToCell(blobBall.transform.position);
        if (false == blobRigidBodies.ContainsKey(gridPosition))
        {
            return; // cell was already converted from a tile to a rigidbody. It must of had 2 or more tiles below it deleted.
        }

        RemoveTile(gridPosition);
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
                DestroyedCellsCleanup();
                break;
            case BlobBall.AfterUsed.AddToGrid:
                // deactivate rigidbody until tile needs to fall
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

    private void Set(TileBase coloredTile, Vector2Int position, int pointValue = -1)
    {
        // show and store point value
        if (pointValue != -1) // add overlay tile
        {
            overlay.SetTile((Vector3Int)position, overlayTiles[pointValue]);
            scoreBlob[position] = (byte)pointValue;
        }

        // link cell with neighboring cells
        this.tilemap.SetTile((Vector3Int)position, coloredTile);
    }

    private void Start()
    {
        initTilemap();

        // Test tilemap rule tiles render
        //Set(whiteTile, new Vector2Int(0, 0), 8);
        //Set(whiteTile, new Vector2Int(1, 0), 9);
        //Set(whiteTile, new Vector2Int(2, 0), 10);
        //Set(redTile, new Vector2Int(3, 0), 10);
        //Set(redTile, new Vector2Int(4, 0), 9);
        //Set(whiteTile, new Vector2Int(5, 0), 10);
        //Set(whiteTile, new Vector2Int(6, 0), 9);
        //Set(whiteTile, new Vector2Int(7, 0), 8
    }
}
