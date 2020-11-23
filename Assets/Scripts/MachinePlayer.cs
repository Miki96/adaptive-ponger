using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class MachinePlayer : Player
{
    public float limit = 1f;
    private SharpML sml;

    public void setModel(SharpML model)
    {
        sml = model;
    }

    public void trainModel(string dataName, string savePath)
    {
        sml.Train(dataName, savePath);
    }

    public override int Move(PongData data)
    {
        double dir = sml.Predict(data) - data.playerPos;

        if (Mathf.Abs((float)dir) < limit) return 0;
        if (dir > 0) return 1;
        if (dir < 0) return -1;
        return 0;
    }
}