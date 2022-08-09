using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public static int enemies;
    public static int players;
    public int enemiesDead;
    public GameOver gameOverCanvas;
    public UImanager uImanager;

    private void Awake()
    {
        enemiesDead = 0;
        players = 1;
        enemies = 2;
        
    }
    private void Start()
    {
        VFX.SetActive(false);
        StartCoroutine(CheckGameRoutine());
    }

    public GameObject VFX;
    void WinRoutine()
    {
        SandmanController.an.SetTrigger("Win");
        VFX.SetActive(true);
    }
    
    IEnumerator CheckGameRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (enemies <= 0)
            {
                //WinLoseText.gameObject.SetActive(true);
                uImanager.EndGame();
                gameOverCanvas.WinScreen.SetActive(true);
                gameOverCanvas.LoseScreen.SetActive(false);
                WinRoutine();
                break;
            }

            if (players<=0)
            {
                uImanager.EndGame();
                gameOverCanvas.WinScreen.SetActive(false);
                gameOverCanvas.LoseScreen.SetActive(true);
                break;
            }
        }
    }

}
