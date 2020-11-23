using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : Player
{
    public override int Move(PongData data)
    {
        if (data.playerPos > data.ballPosX)
        {
            return -1;
        } 
        else if (data.playerPos < data.ballPosX)
        {
            return 1;
        } 
        else
        {
            return 0;
        }
    }
}
