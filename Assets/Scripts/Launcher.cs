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
    public TMP_Text roomNameTxt;

    public GameObject errorScreen;
    public TMP_Text errorTxt;
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
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        loadingText.text = "Joinin Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuBtns.SetActive(true);
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
}
