using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using Photon.Pun;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    public TMP_Text overheatedTxt;
    public Slider weaponTempSlide;
    public Slider playerHealthSlide;

    public GameObject deathScreen;
    public TMP_Text killedByTxt;
    public TMP_Text killsTxt;
    public TMP_Text deathsTxt;

    public GameObject leaderBoard;
    public LeaderBoardItem leaderBoardItemPrefab;
    private List<LeaderBoardItem> listLeaderBoardItem = new List<LeaderBoardItem>();

    public GameObject endScreen;
    public TMP_Text timerTxt;

    public GameObject optionScreen;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && MatchManager.ins.currentState != GameState.ENDING)
        {
            if (leaderBoard.activeSelf)
            {
                leaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
        if (optionScreen.activeSelf && Cursor.lockState != CursorLockMode.None) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }
    public void ShowLeaderBoard()
    {
        leaderBoard.SetActive(true);
        foreach (var item in listLeaderBoardItem)
        {
            Destroy(item.gameObject);
        }
        listLeaderBoardItem.Clear();

        MatchManager.ins.listPlayerInfo.OrderByDescending(x => x.kills);

        foreach (var player in MatchManager.ins.listPlayerInfo)
        {
            LeaderBoardItem item = Instantiate(leaderBoardItemPrefab, leaderBoard.transform);
            item.SetDetails(player.name, player.kills, player.deaths);
            listLeaderBoardItem.Add(item);
        }
    }

    public void PauseUnpause()
    {
        if (!optionScreen.activeSelf)
        {
            optionScreen.SetActive(true);
        }
        else
        {
            optionScreen.SetActive(false);
        }

    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

}
