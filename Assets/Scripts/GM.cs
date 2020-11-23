using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public enum Mode {
    TEACH,
    TRAIN,
    TEST,
    DUEL
}

public class GM : MonoBehaviour
{
    [Header("Gameplay")]

    public int target = 30;
    public float timeSpeed = 1;
    public float moveSpeed = 10;
    public bool autoStart = false;
    private int clockTime;
    public int winScore = 5;

    public GameObject[] stages;

    private PlayerController[] players;
    public GameObject[] playerGameObjects;
    private Transform[] playerTransforms;
    public float posLimit = 8.5f;

    public Ball ball;
    public float ballSpeed = 5;
    private Transform ballTransform;
    private Rigidbody2D ballRigidbody;

    private bool playing = false;
    private bool gameover = false;

    [Header("Interface")]

    public Text info;
    public Text levelTitle;
    public Text[] scoreUI;
    public Text[] nameUI;
    private int[] scores;
    public GameObject pauseUI;
    public GameObject tapUI;
    public Text timeUI;
    public GameObject[] controls;
    public Text gameSpeedUI;

    public GameObject winUI;
    public GameObject lostUI;
    public GameObject trainUI;

    public Button[] trainButtons;
    public GameObject[] trainInfos;

    [Header("AI data")]

    private List<PongData> trainData;
    private List<PongData> roundData;
    public int dataSkip = 5;
    public string dataName;


    private void Awake()
    {
        // set initial gameplay speed
        QualitySettings.vSyncCount = 0;
        Time.timeScale = timeSpeed;
        Application.targetFrameRate = (int)(target * timeSpeed);
        gameSpeedUI.text = timeSpeed.ToString();

        // set ball
        ballTransform = ball.transform;
        ballRigidbody = ball.GetComponent<Rigidbody2D>();

        // set players
        playerTransforms = new Transform[playerGameObjects.Length];
        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            playerTransforms[i] = playerGameObjects[i].transform;
        }

        // set player controllers
        players = new PlayerController[2];
        players[0] = playerGameObjects[0].GetComponent<PlayerController>();
        players[1] = playerGameObjects[1].GetComponent<PlayerController>();

        // reset score
        scores = new int[2];
        scores[0] = 0;
        scores[1] = 0;
        UpdateScore();

        // set gameplay mode
        LoadLevel();

        // start timer
        StartCoroutine("Clock");

        Random.InitState(1);
    }

    private void LoadLevel()
    {
        LevelData level = LevelData.instance;

        // set level
        switch (level.type)
        {
            case LEVEL_TYPE.SOLO:
                levelTitle.text = "LVL " + level.level;
                break;
            case LEVEL_TYPE.DUEL:
                levelTitle.text = "DUEL";
                break;
            case LEVEL_TYPE.TEST:
                levelTitle.text = "TEST";
                break;
        }

        // set names
        switch (level.type)
        {
            case LEVEL_TYPE.SOLO:
                nameUI[0].text = level.playerNames[0];
                if (level.control[1] == PLAYER_TYPE.FOLLOWER)
                    nameUI[1].text = level.control[1].ToString();
                else
                    nameUI[1].text = level.ai[1].ToString();
                break;
            case LEVEL_TYPE.DUEL:
                nameUI[0].text = level.playerNames[0];
                nameUI[1].text = level.playerNames[1];
                break;
            case LEVEL_TYPE.TEST:
                if (level.control[0] == PLAYER_TYPE.FOLLOWER)
                    nameUI[0].text = level.control[0].ToString();
                else
                    nameUI[0].text = level.ai[0].ToString();
                if (level.control[1] == PLAYER_TYPE.FOLLOWER)
                    nameUI[1].text = level.control[1].ToString();
                else
                    nameUI[1].text = level.ai[1].ToString();
                break;
        }


        // set colors
        for (int i = 0; i < 2; i++)
        {
            players[i].setColor(level.playerColors[i]);
            nameUI[i].color = level.playerColors[i];
            scoreUI[i].color = level.playerColors[i];
        }

        // set controls
        if (level.type == LEVEL_TYPE.SOLO)
        {
            controls[0].SetActive(true);
            controls[1].SetActive(false);
        }
        else
        {
            controls[0].SetActive(false);
            controls[1].SetActive(true);
        }

        // set stages
        for (int i = 0; i < 4; i++)
        {
            stages[i].SetActive(i == level.stage);
        }

        // set autostart after score
        autoStart = level.control[0] != PLAYER_TYPE.PLAYER && level.control[1] != PLAYER_TYPE.PLAYER;
        //autoStart = true;

        // set player controlers
        for (int i = 0; i < 2; i++)
        {
            players[i].playerController = level.control[i];
        }

        // machine learning data
        if (LevelData.instance.type == LEVEL_TYPE.SOLO)
        {
            trainData = new List<PongData>(10000);
            roundData = new List<PongData>(2000);
        }

        // set machine learning models
        for (int i = 0; i < 2; i++) 
        {
            if (players[i].playerController == PLAYER_TYPE.MACHINE)
            {
                SharpML sml = new SharpML(level.ai[i], level.dataNames[i]);
                //sml.LoadModel(LevelData.instance.stage, LevelData.instance.playerID);
                //sml.Train(LevelData.instance.dataNames[i]);
                players[i].GetComponent<MachinePlayer>().setModel(sml);
            }
        }

    }

    public void controlPlayback(int step)
    {
        if (step == 0)
        {
            // pause / play
            if (Time.timeScale == 0)
            {
                Time.timeScale = timeSpeed;
                Application.targetFrameRate = (int)(target * timeSpeed);
            } else
            {
                Time.timeScale = 0;
            }
        }
        else
        {
            timeSpeed = Mathf.Clamp(timeSpeed + step, 1, 5);

            Time.timeScale = timeSpeed;
            Application.targetFrameRate = (int)(target * timeSpeed);

            // update ui
            gameSpeedUI.text = timeSpeed.ToString();
        }
    }

    void Update()
    {
        // gameplay loop
        if (!gameover && Time.timeScale > 0)
        {
            if (playing)
            {
                MovePlayers();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space) || autoStart || (Input.GetMouseButton(0) && Input.mousePosition.y < Screen.height / 2))
                {
                    StartBall();
                    playing = true;
                    tapUI.SetActive(false);
                }
            }
        }

        // train on data
        if (Input.GetKeyDown(KeyCode.T))
        {
            SaveData();
            TrainModel();
        }

        // back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                BackToMenu();
            }
            else
            {
                Pause(true);
            }
        }
    }

    private IEnumerator Clock()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (playing)
            {
                clockTime++;
                int mins = clockTime / 60;
                int secs = clockTime % 60;
                timeUI.text = mins + ":" + ((secs < 10) ? "0" : "") + secs;
            }
        }
    }

    private void MovePlayers()
    {
        // PLAYER ONE - DOWN
        PongData d1 = new PongData()
        {
            ballPosX = ballTransform.position.x,
            ballPosY = ballTransform.position.y,
            ballDirX = ballRigidbody.velocity.x,
            ballDirY = ballRigidbody.velocity.y,
            ballSpeed = ball.getSpeed(),
            playerPos = playerTransforms[0].position.x,
            enemyPos = playerTransforms[1].position.x,
        };
        // move
        int command1 = players[0].Move(d1);
        Vector2 pos1 = playerTransforms[0].position;
        pos1.x = Mathf.Clamp(pos1.x + command1 * moveSpeed * Time.deltaTime, -posLimit, posLimit);
        playerTransforms[0].position = pos1;

        // PLAYER TWO - UP
        PongData d2 = new PongData()
        {
            ballPosX = -ballTransform.position.x,
            ballPosY = -ballTransform.position.y,
            ballDirX = -ballRigidbody.velocity.x,
            ballDirY = -ballRigidbody.velocity.y,
            ballSpeed = ball.getSpeed(),
            playerPos = -playerTransforms[1].position.x,
            enemyPos = -playerTransforms[0].position.x,
        };
        // move
        int command2 = players[1].Move(d2);
        Vector2 pos2 = playerTransforms[1].position;
        pos2.x = Mathf.Clamp(pos2.x - command2 * moveSpeed * Time.deltaTime, -posLimit, posLimit);
        playerTransforms[1].position = pos2;


        // TEACH IF NEEDED
        if (LevelData.instance.type == LEVEL_TYPE.SOLO)
        {
            d1.playerPos = playerTransforms[0].position.x;
            //d1.Normalize();
            roundData.Add(d1);
            info.text = "ROUND: " + roundData.Count + "\nLEVEL: " + trainData.Count;
        }
    }

    private void StartBall()
    {
        ball.StartBounce(LevelData.instance.level == 0 && LevelData.instance.type == LEVEL_TYPE.SOLO);
    }

    public void Score(int index)
    {
        Reset();

        // update score
        scores[index]++;
        UpdateScore();

        // TEACH IF SCORED
        if (LevelData.instance.type == LEVEL_TYPE.SOLO)
        {
            if (index == 0)
            {
                trainData.AddRange(roundData);
            }
            roundData.Clear();
        }

        // check if end
        if (scores[index] == winScore)
        {
            endGame();
        }
    }

    private void endGame()
    {
        gameover = true;
        StopCoroutine("Clock");
        //EditorApplication.Beep();

        // show screen
        if (scores[0] == winScore)
        {
            winUI.SetActive(true);
        } else
        {
            lostUI.SetActive(true);
        }
    }

    private void Reset()
    {
        playing = false;

        // reset ball
        ball.ResetPosition();

        // reset players
        for (int i = 0; i < players.Length; i++)
        {
            Vector2 pos = playerTransforms[i].position;
            pos.x = 0;
            playerTransforms[i].position = pos;
        }

        // show tap if solo
        if (LevelData.instance.type == LEVEL_TYPE.SOLO)
        {
            tapUI.SetActive(true);
        }
    }

    private void UpdateScore()
    {
        for (int i = 0; i < 2; i++)
        {
            scoreUI[i].text = scores[i].ToString();
        }
    }

    IEnumerator Training()
    {
        trainUI.SetActive(true);
        yield return null;
        // train
        SaveData();
        TrainModel();

        // enable menu
        for (int i = 0; i < trainButtons.Length; i++)
        {
            trainButtons[i].interactable = true;
            trainButtons[i].GetComponentInChildren<Text>().color = new Color(0.2f, 0.2f, 0.2f, 1);
        }
        trainInfos[0].SetActive(false);
        trainInfos[1].SetActive(true);
    }


    public void UpgradeMenu()
    {
        StartCoroutine("Training");
    }

    public void NextLevel()
    {
        LevelData.instance.level++;
        LevelData.instance.info.progress++;
        LevelData.instance.info.Save();
        // save player
        Restart();
    }

    public void Pause(bool pause)
    {
        Time.timeScale = pause ? 0 : timeSpeed;
        pauseUI.SetActive(pause);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SaveData()
    {
        // save data
        if (trainData.Count != 0)
        {
            StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + dataName + ".csv");
            Debug.Log(Application.persistentDataPath);

            sw.WriteLine("f1,f2,f3,f4,f5,f6");
            for (int i = 0; i < trainData.Count; i += dataSkip)
            {
                sw.WriteLine(trainData[i]);
            }
            sw.Close();
        }
    }

    public void TrainModel()
    {
        players[1].GetComponent<MachinePlayer>().trainModel(
            dataName,
            "" + LevelData.instance.info.id + ".model");
    }
}
