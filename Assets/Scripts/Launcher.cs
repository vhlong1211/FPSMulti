using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher ins;
    private void Awake()
    {
        ins = this;
    }

    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject menuBtns;

    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;

    public GameObject roomScreen;
    public Transform playerNameHolder;
    public TMP_Text roomNameTxt;
    public TMP_Text playerNamePrefab;
    public List<TMP_Text> listPlayersName = new List<TMP_Text>();

    public GameObject errorScreen;
    public TMP_Text errorTxt;

    public GameObject roomBrowserScreen;
    public RoomButton roomBtnPrefab;
    public Transform roomBtnHolder;
    private List<RoomButton> roomBtnList = new List<RoomButton>();

    public GameObject namePlayerScreen;
    public TMP_InputField playerNameInput;
    private bool hasSetPlayerName;

    public string levelToPlay;
    public GameObject startButton;

    private void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to network...";

        PhotonNetwork.ConnectUsingSettings();
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuBtns.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        namePlayerScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joinin Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuBtns.SetActive(true);

        if (!hasSetPlayerName)
        {
            CloseMenus();
            namePlayerScreen.SetActive(true);

            if (PlayerPrefs.HasKey("Player_Name"))
            {
                playerNameInput.text = PlayerPrefs.GetString("Player_Name");
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("Player_Name");
            }
        } 
    }

    public void OnClickCreateRoom()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void OnClickCreateRoomReal()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom(roomNameInput.text, options);

        CloseMenus();
        loadingText.text = "Creating Room...";
        loadingScreen.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text playerName = Instantiate(playerNamePrefab, playerNameHolder);
        playerName.text = newPlayer.NickName;

        listPlayersName.Add(playerName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    private void ListAllPlayers()
    {
        foreach (var a in listPlayersName)
        {
            Destroy(a.gameObject);
        }
        listPlayersName.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text playerName = Instantiate(playerNamePrefab, playerNameHolder);
            playerName.text = players[i].NickName;

            listPlayersName.Add(playerName);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorTxt.text = "Failed to Create Room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public void OnClickCloseErrorScreen()
    {
        CloseMenus();
        menuBtns.SetActive(true); 
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuBtns.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuBtns.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in roomBtnList)
        {
            Destroy(rb.gameObject);
        }

        roomBtnList.Clear();

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton btn = Instantiate(roomBtnPrefab,roomBtnHolder);
                btn.SetButtonDetails(roomList[i]);
                roomBtnList.Add(btn);
            }
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        CloseMenus();
        loadingText.text = "Joining Room";
        loadingScreen.SetActive(true);
    }

    public void OnClickSetNickName()
    {
        if (!string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;

            PlayerPrefs.SetString("Player_Name", playerNameInput.text);
            CloseMenus();
            menuBtns.SetActive(true);
            hasSetPlayerName = true;
        }
    }

    public void OnClickStartGame()
    {
        PhotonNetwork.LoadLevel(levelToPlay);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else 
        {
            startButton.SetActive(false);
        }
    }

    public void OnClickQuitGame()
    {
        Application.Quit();
    }
}
