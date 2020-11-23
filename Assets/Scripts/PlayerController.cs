using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PLAYER_TYPE
{
    PLAYER,
    FOLLOWER,
    MACHINE,
}

public class PlayerController : MonoBehaviour
{
    private MachinePlayer machinePlayer;
    private KeyboardPlayer keyboardPlayer;
    private FollowPlayer followPlayer;

    public PLAYER_TYPE playerController;

    private SpriteRenderer sprite;
    private SpriteRenderer shadow;

    void Awake()
    {
        machinePlayer = GetComponent<MachinePlayer>();
        keyboardPlayer = GetComponent<KeyboardPlayer>();
        followPlayer = GetComponent<FollowPlayer>();

        // color
        sprite = GetComponent<SpriteRenderer>();
        shadow = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public int Move(PongData data)
    {
        Player player = null;

        switch (playerController)
        {
            case PLAYER_TYPE.MACHINE:
                player = machinePlayer;
                break;
            case PLAYER_TYPE.PLAYER:
                player = keyboardPlayer;
                break;
            case PLAYER_TYPE.FOLLOWER:
                player = followPlayer;
                break;
        }

        return player.Move(data);
    }

    public void setColor(Color c)
    {
        sprite.color = c;
        shadow.color = new Color(c.r, c.g, c.b, 0.25f);
    }

}
