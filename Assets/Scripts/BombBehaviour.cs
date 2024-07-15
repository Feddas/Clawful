using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : DropBall
{
    public override void PowerActivation()
    {
        if(nodeLocation.nodeLeft != null && nodeLocation.nodeLeft.content != null)
            nodeLocation.nodeLeft.GroupDestroy();
        if (nodeLocation.nodeBelow != null && nodeLocation.nodeBelow.content != null)
            nodeLocation.nodeBelow.GroupDestroy();
        if (nodeLocation.nodeRight != null && nodeLocation.nodeRight.content != null)
            nodeLocation.nodeRight.GroupDestroy();
        nodeLocation.BaseDestroy();
    }
}
