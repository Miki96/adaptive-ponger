using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public float[] color;
    public int level;
    public int progress;
    public int id;

    public void Save()
    {
        string path = Application.persistentDataPath + "/" + level + "/" + id;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = new FileStream(path, FileMode.Create);
        bf.Serialize(file, this);
        file.Close();
    }
}
