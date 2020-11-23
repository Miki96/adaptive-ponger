using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LEVEL_TYPE
{
    SOLO,
    DUEL,
    TEST
}

public enum MACHINE_TYPE
{
    PLAYER,

    DECISION_TREE,
    RANDOM_FOREST,
    EXTRA_TREES,

    ADABOOST,

    GRAD_SQUARE,
    GRAD_ABSOLUTE,
    GRAD_HUBER,
    GRAD_QUANTILE,

    NEURAL_NETWORK
}

public class LevelData : MonoBehaviour
{
    public static LevelData instance;

    public string[] playerNames;
    public Color[] playerColors;

    public int level;
    public int stage;

    public LEVEL_TYPE type;

    public PLAYER_TYPE[] control;

    public MACHINE_TYPE[] ai;

    public string[] dataNames;

    public PlayerInfo info;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
