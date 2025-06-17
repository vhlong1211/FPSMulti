using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
}
