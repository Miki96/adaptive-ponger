using UnityEngine;
using System.Collections;
using System.IO;

public class KeyboardPlayer : Player
{
    public KeyCode rightInput;
    public KeyCode leftInput;

    public override int Move(PongData data)
    {
        int right = Input.GetKey(rightInput) ? 1 : 0;
        int left = Input.GetKey(leftInput) ? 1 : 0;
        int dir = right - left;

        if (Input.GetMouseButton(0))
        {
            Vector2 mouse = Input.mousePosition;
            if (mouse.y < Screen.height / 2)
            {
                right = mouse.x > Screen.width / 2 ? 1 : 0;
                left = mouse.x < Screen.width / 2 ? 1 : 0;
                dir = right - left;
            }
        }

        return dir;
    }
}
