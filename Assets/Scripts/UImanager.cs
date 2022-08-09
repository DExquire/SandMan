using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UImanager : MonoBehaviour
{    
    public int ResumeTimes = 0;
    public int LvlNumber;
    public string LevelName;
    public bool ShowLabel = false;

    //Canvas group
    public GameObject GamePause;
    public GameObject GamePlay;
    public GamePlay gamePlay;
    public GameObject GameOver;

    public GameObject clipper;

    private TouchPhase touchPhase;

    public int isGameFinished = 0;
    
    public enum LevelNumber
    {
        lvl_1,
        lvl_2,
        lvl_3,
        lvl_4,
        lvl_5,
        lvl_6,
        lvl_7,
    }

    private void OnEnable() 
    {
        ResumeTimes++;
        ShowLabel = true;
        
        LevelName = SceneManager.GetActiveScene().name;
        LvlNumber = GetLevelNumber(SceneManager.GetActiveScene().name);
        if(LvlNumber == 7 && isGameFinished == 0)
        {
            isGameFinished = 1;
            PlayerPrefs.SetInt("isRandom", isGameFinished);
        }
        
        isGameFinished = PlayerPrefs.GetInt("isRandom");
        ResumeGame();
    }
    
    private void Update() 
    {
        if (TouchUtility.TouchCount > 0)
        {
            Touch touch = TouchUtility.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                gamePlay.Label.gameObject.SetActive(false);
                gamePlay.Hand.gameObject.SetActive(false);
                gamePlay.TapAndDrag.gameObject.SetActive(false);
                gamePlay.TapToPlay.gameObject.SetActive(false);
            }
        }
        
    }

    private string GetLevelName(LevelNumber levelNumber)
    {
        switch (levelNumber)
        {
            case LevelNumber.lvl_1:
            return "lvl-1";
            case LevelNumber.lvl_2:
            return "lvl-2";
            case LevelNumber.lvl_3:
            return "lvl-3";
            case LevelNumber.lvl_4:
            return "lvl-4";
            case LevelNumber.lvl_5:
            return "lvl-5";
            case LevelNumber.lvl_6:
            return "lvl-6";
            case LevelNumber.lvl_7:
            return "lvl-7";
            default:
            return "lvl-1";
        }
    }

    private int GetLevelNumber(string levelName)
    {
        string[] number = levelName.Split(char.Parse("-"));
        int levelNumber;
        int.TryParse(number[1], out levelNumber);
        return levelNumber--;
    }

    public void LoadLvl(string lvlNmbr)
    {
        if(isGameFinished == 0)
        {
            lvlNmbr = GetLevelName((LevelNumber)LvlNumber++);
        }
        else
        {
            lvlNmbr = GetLevelName((LevelNumber)RandomLevel());
        }
        SceneManager.LoadScene(lvlNmbr, LoadSceneMode.Single);
        ResumeTimes = 0;
    }

    public void ReloadLvl(string lvlNmbr)
    {
        string lvlNmbr1 = GetLevelName((LevelNumber)LvlNumber-1);
        SceneManager.LoadScene(lvlNmbr1, LoadSceneMode.Single);
        ResumeTimes = 0;
    }

    public void ResumeGame()
    {
        gamePlay.gameObject.SetActive(true);
        if(ResumeTimes == 1 && ShowLabel)
        {
            gamePlay.Label.text = "LEVEL " + (GetLevelNumber(LevelName));            

            gamePlay.TapAndDrag.SetActive(true);

            ShowLabel = false;
        }
        GamePause.SetActive(false);
        GameOver.SetActive(false);
        clipper.SetActive(true);
    }
    public void PauseGame()
    {
        gamePlay.gameObject.SetActive(false);
        GamePause.SetActive(true);
        GameOver.SetActive(false);
        clipper.SetActive(false);
    }

    public void EndGame()
    {
        GamePlay.SetActive(false);
        GamePause.SetActive(false);
        GameOver.SetActive(true);
        clipper.SetActive(false);
        
    }

    public int RandomLevel()
    {
        int CurrentLevelNumber = GetLevelNumber(LevelName);
        int NextLevelNumber = Random.Range(0, 6);
        if(CurrentLevelNumber == NextLevelNumber)
        {
            RandomLevel();
        }
        return NextLevelNumber;        
    }

    public void ExitApp()
    {
        Application.Quit();
    }
    
    
}
    
    
