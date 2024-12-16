using UnityEngine;

public class BombBall : BlobBall
{
    public override AfterUsed PowerActivation(Vector2Int me, ScoreBlob score)
    {
        score.ScoreTilesLinkedTo(me + Vector2Int.left);
        score.ScoreTilesLinkedTo(me + Vector2Int.right);
        score.ScoreTilesLinkedTo(me + Vector2Int.down);
        return AfterUsed.Destroy;
    }
}
