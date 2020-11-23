using UnityEngine;
using UnityEditor;

public class PongData
{
    public float ballPosX { get; set; }
    public float ballPosY { get; set; }
    public float ballDirX { get; set; }
    public float ballDirY { get; set; }
    public float ballSpeed { get; set; }
    public float playerPos { get; set; }
    public float enemyPos { get; set; }

    public override string ToString()
    {
        return ballPosX + "," + ballPosY + "," + ballDirX + "," + ballDirY + "," + playerPos + "," + enemyPos;
    }

    public void Normalize()
    {
        ballPosX = ((ballPosX / 9.4f) + 1.0f) / 2.0f;
        ballPosY = ((ballPosY / 14.4f) + 1.0f) / 2.0f;

        ballDirX = ((ballDirX / 25f) + 1.0f) / 2.0f;
        ballDirY = ((ballDirY / 25f) + 1.0f) / 2.0f;

        playerPos = ((playerPos / 8.5f) + 1.0f) / 2.0f;
        enemyPos = ((enemyPos / 8.5f) + 1.0f) / 2.0f;
    }

    public static double RealPos(double pos)
    {
        return ((pos * 2.0) - 1.0) * 8.5;
    }
}