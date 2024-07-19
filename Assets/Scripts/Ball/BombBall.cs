using UnityEngine;

public class BombBall : BlobBall
{
    public override AfterUsed PowerActivation(Vector2Int me, ScoreBlob score)
    {
        score.ScoreTilesLinkedTo(me + Vector2Int.left);
        score.ScoreTilesLinkedTo(me + Vector2Int.right);
        score.ScoreTilesLinkedTo(me + Vector2Int.down);
        return AfterUsed.Destroy;

        //if (nodeLocation.nodeLeft != null && nodeLocation.nodeLeft.content != null)
        //    nodeLocation.nodeLeft.GroupDestroy();
        //if (nodeLocation.nodeBelow != null && nodeLocation.nodeBelow.content != null)
        //    nodeLocation.nodeBelow.GroupDestroy();
        //if (nodeLocation.nodeRight != null && nodeLocation.nodeRight.content != null)
        //    nodeLocation.nodeRight.GroupDestroy();
        //nodeLocation.BaseDestroy();
    }
}
