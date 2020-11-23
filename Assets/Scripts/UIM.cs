using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

public class UIM : MonoBehaviour
{
    public GameObject[] pages;
    private int currentPage = 0;

    public InputField heroName;
    public Color heroColor;

    private PlayerInfo[] players;

    public GameObject toggleParent;

    public Transform savesParent;
    public GameObject playerSave;

    // Start is called before the first frame update
    void Start()
    {
        initDatabase();
        initToggles();
    }

    // Update is called once per frame
    void Update()
    {
        // back button
        if (Input.GetKey(KeyCode.Escape))
        {
            if (currentPage == 0)
            {
                Application.Quit();
            }
            else
            {
                ChangePage(currentPage - 1);
            }
        }
    }

    private void initToggles()
    {
        Toggle[] toggles = toggleParent.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            toggles[i].onValueChanged.AddListener((bool state) =>
            {
                colorChanged(toggles[index].GetComponent<Image>().color);
            });
        }
    }

    public void ExitButton()
    {
        print("exit");
        Application.Quit();
    }

    public void LevelSelected(int level)
    {
        // save level selection
        LevelData.instance.stage = level;

        // load data from players
        string path = Application.persistentDataPath + "/" + level;
        var info = new DirectoryInfo(path);
        var files = info.GetFiles();

        players = new PlayerInfo[files.Length];
        BinaryFormatter bf = new BinaryFormatter();
        for (int i = 0; i < players.Length; i++)
        {
            FileStream file = new FileStream(path + "/" + files[i].Name, FileMode.Open);
            players[i] = (PlayerInfo)bf.Deserialize(file);
            file.Close();
        }

        // fill players data
        foreach (Transform child in savesParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            print(players[i].name);
            int index = i;
            Transform save = Instantiate(playerSave, savesParent).transform;
            // info
            float[] c = players[i].color;
            save.GetChild(0).GetComponent<Text>().text = players[i].name;
            save.GetChild(1).GetComponent<Image>().color = new Color(c[0], c[1], c[2], 1);
            save.GetChild(2).GetComponent<Text>().text = players[i].progress.ToString();
            // button
            save.GetComponent<Button>().onClick.AddListener(() =>
            {
                HeroSelected(index);
            });
        }
    }

    public void colorChanged(Color c)
    {
        heroColor = c;
    }

    public void HeroSelected(int hero)
    {
        LoadHero(players[hero]);
        PlayGame();
    }

    public void LoadHero(PlayerInfo player)
    {
        LevelData.instance.playerNames[0] = player.name;
        LevelData.instance.level = player.progress;
        float[] c = player.color;
        LevelData.instance.playerColors[0] = new Color(c[0], c[1], c[2], 1);
        LevelData.instance.info = player;

        LevelData.instance.dataNames[1] = player.id + ".model";
    }

    public void HeroCreated()
    {
        // get id and increase
        int id = PlayerPrefs.GetInt("id");
        PlayerPrefs.SetInt("id", id + 1);

        // create hero
        PlayerInfo player = new PlayerInfo();
        player.color = new float[] { heroColor.r, heroColor.g, heroColor.b };
        player.name = heroName.text;
        player.progress = 0;
        player.level = LevelData.instance.stage;
        player.id = id;

        // save hero
        player.Save();

        // load hero
        LoadHero(player);

        // play
        PlayGame();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void test()
    {
        print("TEST");
    }

    public void ChangePage(int page)
    {
        currentPage = page;
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == page);
        }
    }

    private void initDatabase()
    {
        string path = Application.persistentDataPath;
        print(path);

        // init id generator
        if (!PlayerPrefs.HasKey("id"))
        {
            PlayerPrefs.SetInt("id", 0);
        }

        // create level folders
        for (int i = 0; i < 4; i++)
        {
            string dir = path + "/" + i;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
