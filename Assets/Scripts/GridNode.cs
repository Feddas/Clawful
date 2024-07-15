using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnityEditor.ShortcutManagement;

public class GridNode : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public float nodeSize = 1f;
    private GridManager gridManager;

    private float time = 0;
    private float columnDuration = 0.2f;

    public GridNode nodeRight;
    public GridNode nodeLeft;
    public GridNode nodeAbove;
    public GridNode nodeBelow;

    public bool rightKnown;
    public bool leftKnown;
    public bool aboveKnown;
    public bool bottomKnown;

    public bool isProcessing;
    public bool isOccupied;
    //public MatchChecker content;
    public DropBall content;
    public bool isGrouped;
    public bool isMaster;
    public GridNode masterNode;
    public List<GridNode> connectedNodes;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        nodeBelow = gridManager.GetNodeAtPosition(gridX, gridY + 1);
        nodeAbove = gridManager.GetNodeAtPosition(gridX, gridY - 1);
        nodeRight = gridManager.GetNodeAtPosition(gridX + 1, gridY);
        nodeLeft = gridManager.GetNodeAtPosition(gridX - 1, gridY);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //switch from match checker when new class is made
        DropBall db = other.GetComponent<DropBall>();
        //isProcessing used to make sure drops don't overlap when they trigger the node. Only necessary if not occupied
        if (!isOccupied && !isProcessing)
        {
            if (gridY == 0 && !db.isInGrid && !db.isClaimed)
            {
                isProcessing = true;
                db.isClaimed = true;
                db.isInGrid = true;
                time = 0;
                StartCoroutine(CenterColumn(other.transform));
            }

            // Check if the space below is empty
            if (nodeBelow != null && !nodeBelow.isOccupied)
            {
                // Allows the target to fall if node below is unoccupied
                return;
            }
            else
            {
                //claim the target, mark self as occupied, center target on node.
                isOccupied = true;
                db.isInGrid = true;
                db.nodeLocation = this;
                CenterBall(other.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DropBall db = other.GetComponent<DropBall>();
        //if not occupied and nodeBelow is occupied, claim and center target and mark self as occupied.
        if (!isOccupied && !isProcessing)
        {
            if (!db.isClaimed && nodeBelow != null && nodeBelow.isOccupied || !db.isClaimed && nodeBelow == null)
            {
                isProcessing = true;
                isOccupied = true;
                content = db;
                db.isInGrid = true;
                db.isClaimed = true;
                db.nodeLocation = this;
                StartCoroutine(CenterColumn(other.transform));
            }
        }
    }

    IEnumerator CenterColumn(Transform other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        Vector2 start = other.position;
        Vector2 end = transform.position;
        Vector3 sm = other.localScale;
        Vector3 mm = Vector3.one;

        while (time < columnDuration)
        {
            other.position = Vector2.Lerp(start, end, time / columnDuration);
            other.rotation = Quaternion.Lerp(other.rotation, Quaternion.identity, time / columnDuration);
            if (isOccupied)
                other.localScale = Vector3.Lerp(sm, mm, time / columnDuration);
            time += Time.deltaTime;
            yield return null;
        }
        other.position = end;

        other.localScale = Vector3.one;

        rb.velocity = new Vector2(0, rb.velocity.y);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        
        if (isOccupied && content.isMatcher)
        {
            CheckNeighbors();
        }
        else if (isOccupied && !content.isMatcher)
        {
            content.PowerActivation();
        }

        isProcessing = false;
    }

    private void CenterBall(GameObject other)
    {
        Debug.Log("Centered");
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        //content = other.GetComponent<MatchChecker>();
        content = other.GetComponent<DropBall>();
        content.isInGrid = true;
        content.isClaimed = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        time = 0;
        StartCoroutine(CenterColumn(other.transform));
    }

    public void CheckNeighbors()
    {
        if (nodeBelow != null && nodeBelow.isOccupied && !nodeBelow.isProcessing && !bottomKnown && nodeBelow.content != null && nodeBelow.content.colorTag == content.colorTag)
        {
            content.matchBottom = true;
            bottomKnown = true;
            GridNode g = nodeBelow;
            g.content.matchTop = true;
            g.aboveKnown = true;
            g.content.MatchCheck();
            GroupCheck(g);
        }

        if (nodeLeft != null && nodeLeft.isOccupied && !nodeLeft.isProcessing && !leftKnown && nodeLeft.content != null && nodeLeft.content.colorTag == content.colorTag)
        {
            content.matchLeft = true;
            leftKnown = true;
            GridNode g = nodeLeft;
            g.content.matchRight = true;
            g.rightKnown = true;
            g.content.MatchCheck();
            GroupCheck(g);
        }

        if (nodeRight != null && nodeRight.isOccupied && !nodeRight.isProcessing && !rightKnown && nodeRight.content != null && nodeRight.content.colorTag == content.colorTag)
        {
            content.matchRight = true;
            rightKnown = true;
            GridNode g = nodeRight;
            g.content.matchLeft = true;
            g.leftKnown = true;
            g.content.MatchCheck();
            GroupCheck(g);
        }

        if (nodeAbove != null && nodeAbove.isOccupied && !nodeAbove.isProcessing && !aboveKnown && nodeAbove.content != null && nodeAbove.content.colorTag == content.colorTag)
        {
            content.matchTop = true;
            aboveKnown = true;
            GridNode g = nodeAbove;
            g.content.matchBottom = true;
            g.bottomKnown = true;
            g.content.MatchCheck();
            GroupCheck(g);
        }

        if (aboveKnown || leftKnown || rightKnown || bottomKnown)
        {
            content.MatchCheck();
        }
    }

    public void GroupCheck(GridNode g)
    {
        if (!isGrouped)
        {
            if (g.isGrouped)
            {
                if (g.isMaster)
                {
                    if (g.connectedNodes != null)
                        JoinGroup(g);
                    else
                        Debug.LogError("False master " + g);
                }
                else if (g.masterNode != null)
                {
                    JoinGroup(g.masterNode);
                }
            }
            else
            {
                CreateGroup(g);
            }
        }
        else
        {
            if (g.isGrouped)
            {
                if (g.masterNode != masterNode)
                {
                    if (isMaster)
                    {
                        if (g.isMaster)
                        {
                            TransferGroup(g);
                        }
                        else if (g.masterNode != this)
                        {
                            TransferGroup(g.masterNode);
                        }
                    }
                    else
                    {
                        if (g.isMaster && masterNode != g)
                        {
                            masterNode.TransferGroup(g);
                        }
                        else if (g.masterNode != masterNode)
                        {
                            masterNode.TransferGroup(g.masterNode);
                        }
                    }
                }
            }
            else
            {
                if (!isMaster)
                    g.JoinGroup(masterNode);
                else
                    g.JoinGroup(this);
            }
        }
    }

    public void CreateGroup(GridNode g)
    {
        isMaster = true;
        isGrouped = true;
        connectedNodes.Add(this);
        connectedNodes.Add(g);
        g.isGrouped = true;
        g.masterNode = this;
    }

    public void JoinGroup(GridNode g)
    {
        if (!g.isMaster)
        {
            Debug.LogError("Trying to join non master group " + g);
        }

        isGrouped = true;
        masterNode = g;
        masterNode.connectedNodes.Add(this);
    }

    //only masters should transfer
    public void TransferGroup(GridNode g)
    {
        isMaster = false;
        for (int n = 0; n < connectedNodes.Count; n++)
        {
            connectedNodes[n].masterNode = g;
            g.connectedNodes.Add(connectedNodes[n]);
        }
        masterNode = g;
        connectedNodes.Clear();
    }

    public void RemoveFromGroup()
    {
        if (!isMaster)
        {
            masterNode.connectedNodes.Remove(this);
            masterNode = null;
        }
        isGrouped = false;
        isOccupied = false;
    }

    public void DestroyNodeContent()
    {
        if (isOccupied && content.isMatcher)
        {
            if (isGrouped)
            {
                RemoveFromGroup();
            }
            //add VFX activation and destruction of the ball from here
            content = null;

            if (aboveKnown || nodeAbove.content != null && nodeAbove.content.isMatcher)
            {
                nodeAbove.ReleaseContent();
            }
            if (nodeLeft.content.isMatcher)
                nodeLeft.CheckNeighbors();
            if (nodeRight.content.isMatcher)
                nodeRight.CheckNeighbors();

            if (nodeBelow != null)
            {
                if (nodeBelow.content.isMatcher)
                    nodeBelow.CheckNeighbors();
            }
        }
    }

    public void ReleaseContent()
    {
        if (isOccupied && content.isMatcher)
        {
            //print("Release " + content.gameObject.name);
            content.LoseConnection();
            if (isGrouped)
            {
                RemoveFromGroup();
            }
            content.ClearMatches();

            if (nodeAbove != null && nodeAbove.content != null)
            {
                nodeAbove.ReleaseContent();
            }

            if (nodeLeft != null && nodeLeft.content != null && nodeLeft.content.colorTag == content.colorTag)
            {
                nodeLeft.ResetMatches();
            }

            if (nodeRight != null && nodeRight.content != null && nodeRight.content.colorTag == content.colorTag)
            {
                nodeRight.ResetMatches();
            }

            if (nodeBelow != null && nodeBelow.content != null && nodeBelow.content.colorTag == content.colorTag)
            {
                nodeBelow.ResetMatches();
            }
            ResetNode();
        }
    }

    public void ClearContent()
    {
        if (isOccupied && content.isMatcher)
        {
            ResetNode();
            if (nodeAbove != null && nodeAbove.content != null)
            {
                nodeAbove.ReleaseContent();
            }

            //if (nodeLeft != null && nodeLeft.content != null && nodeLeft.content.colorTag == content.colorTag)
            //{
            //    nodeLeft.content.ClearMatches();
            //    nodeLeft.CheckNeighbors();
            //}

            //if (nodeRight != null && nodeRight.content != null && nodeRight.content.colorTag == content.colorTag)
            //{
            //    nodeRight.content.ClearMatches();
            //    nodeRight.CheckNeighbors();
            //}

            //if (nodeBelow != null && nodeBelow.content != null && nodeBelow.content.colorTag == content.colorTag)
            //{
            //    nodeBelow.content.ClearMatches();
            //    nodeBelow.CheckNeighbors();
            //}
        }
    }

    public void CalculateScore()
    {
        int groupTotal = 0;

        if (isGrouped)
        {
            if (isMaster)
            {
                for (int i = 0; i < connectedNodes.Count; i++)
                {
                    if (!connectedNodes[i].content.isMultiplier)
                        groupTotal += connectedNodes[i].content.pointValue;
                }

                for (int i = 0; i < connectedNodes.Count; i++)
                {
                    if (connectedNodes[i].content.isMultiplier)
                        groupTotal = groupTotal * connectedNodes[i].content.pointValue;
                }

                gridManager.score += groupTotal;
            }
        }
        else
        {
            if (!content.isMultiplier)
            {
                gridManager.score += content.pointValue;
            }
        }
        gridManager.UpdateScore();
    }

    public void ScoreActivate()
    {
        if (!isMaster)
        {
            if (isGrouped)
            {
                if (masterNode != null && !masterNode.isProcessing)
                    masterNode.ScoreActivate();
            }
            else
            {
                CalculateScore();
            }
        }
        else
        {
            isProcessing = true;
            CalculateScore();
        }
    }

    public void GroupDestroy()
    {
        if (!isMaster)
        {
            if (masterNode != null && !masterNode.isProcessing)
                masterNode.GroupDestroy();
            else if (!isGrouped)
            {
                CalculateScore();
                Destroy(content.gameObject);
                ResetNode();
            }
        }
        else
        {
            isProcessing = true;
            if (content != null && content.isMatcher)
            {
                CalculateScore();
                if (isGrouped)
                {
                    for (int i = 0; i < connectedNodes.Count; i++)
                    {
                        Destroy(connectedNodes[i].content.gameObject);
                    }

                    for (int i = 0; i < connectedNodes.Count; i++)
                    {
                        connectedNodes[i].ClearContent();
                    }
                }
            }
        }
    }

    public void BaseDestroy()
    {
        Destroy(content.gameObject);
        ResetNode();
    }

    public void ResetMatches()
    {
        aboveKnown = false;
        bottomKnown = false;
        rightKnown = false;
        leftKnown = false;
        content.ClearMatches();
        CheckNeighbors();
    }

    public void ResetNode()
    {
        isOccupied = false;
        isProcessing = false;
        isMaster = false;
        isGrouped = false;
        aboveKnown = false;
        bottomKnown = false;
        rightKnown = false;
        leftKnown = false;
        content = null;
        if (nodeAbove != null && nodeAbove.content != null)
        {
            nodeAbove.ReleaseContent();
        }
    }
}
