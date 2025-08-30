using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.ReloadAttribute;
using Unity.VisualScripting;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager ins;
    private void Awake()
    {
        ins = this;
    }

    public List<PlayerInfo> listPlayerInfo = new List<PlayerInfo>();
    private int index;

    public int killsToWin;
    public Transform endCamPos;
    public GameState currentState = GameState.PLAYING;
    public float waitAfterEnding = 5f;

    public float matchLength = 180f;
    private float currentMatchTime;
    private float sendTimer;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            SetupTimer();
        }
    }

    private void Update()
    {
        if (currentMatchTime > 0 && currentState == GameState.PLAYING)
        {
            currentMatchTime -= Time.deltaTime;
            if (currentMatchTime < 0)
            {
                currentMatchTime = 0;
                currentState = GameState.ENDING;
                if (PhotonNetwork.IsMasterClient)
                {
                    UpdateGameStateSend(2);
                }
            }
            UpdateTimerDisplay();

            if (PhotonNetwork.IsMasterClient)
            {
                sendTimer -= Time.deltaTime;
                if (sendTimer < 0)
                {
                    sendTimer += 1f;
                    TimerSend();
                }
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventType eventType = (EventType)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            if (eventType == EventType.NEW_PLAYER)
            {
                NewPlayerReceive(data);
            }
            else if (eventType == EventType.LIST_PLAYER)
            {
                ListPlayerReceive(data);
            }
            else if (eventType == EventType.UPDATE_STAT)
            {
                UpdateStatReceive(data);
            }
            else if (eventType == EventType.UPDATE_GAME_STATE)
            {
                UpdateGameStateReceive(data);
            }
            else if (eventType == EventType.TIMER_SYNC)
            {
                TimerReceive(data);
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];

        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent((byte)EventType.NEW_PLAYER,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo playerInfo = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        listPlayerInfo.Add(playerInfo);
        ListPlayerSend();

        if (PhotonNetwork.IsMasterClient)
            UpdateGameStateSend((int)currentState);
    }

    public void ListPlayerSend()
    {
        object[] package = new object[listPlayerInfo.Count];
        for (int i = 0; i < listPlayerInfo.Count; i++)
        {
            object[] data = new object[4];
            data[0] = listPlayerInfo[i].name;
            data[1] = listPlayerInfo[i].actor;
            data[2] = listPlayerInfo[i].kills;
            data[3] = listPlayerInfo[i].deaths;

            package[i] = data;
        }

        PhotonNetwork.RaiseEvent((byte)EventType.LIST_PLAYER,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayerReceive(object[] dataReceived)
    {
        listPlayerInfo.Clear();

        for (int i = 0; i < dataReceived.Length; i++)
        {
            object[] data = (object[])dataReceived[i];

            PlayerInfo p = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);

            listPlayerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
            {
                index = i;
            }
        }
    }

    public void UpdateStatSend(int actor, int type, int amountChange)
    {
        object[] package = new object[3] { actor, type, amountChange };

        PhotonNetwork.RaiseEvent((byte)EventType.UPDATE_STAT,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateStatReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int type = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for (int i = 0; i < listPlayerInfo.Count; i++)
        {
            if (listPlayerInfo[i].actor == actor)
            {
                if (type == 0)
                {
                    listPlayerInfo[i].kills += amount;
                }
                else if (type == 1)
                {
                    listPlayerInfo[i].deaths += amount;
                }

                if (i == index)
                    UpdateStatDisplay();
            }
        }
        ScoreCheck();
    }

    public void UpdateGameStateSend(int state)
    {
        object[] package = new object[1] { state };

        PhotonNetwork.RaiseEvent((byte)EventType.UPDATE_GAME_STATE,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateGameStateReceive(object[] dataReceived)
    {
        int state = (int)dataReceived[0];

        currentState = (GameState)state;

        if (currentState == GameState.ENDING)
        {
            EndGame();
        }
    }

    public void TimerSend()
    {
        object[] package = new object[1] { (int)currentMatchTime };
        PhotonNetwork.RaiseEvent((byte)EventType.TIMER_SYNC,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        UpdateTimerDisplay();
    }

    public void UpdateStatDisplay()
    {
        UIManager.ins.killsTxt.text = "Kills: "+ listPlayerInfo[index].kills.ToString();
        UIManager.ins.deathsTxt.text = "Deaths: " + listPlayerInfo[index].deaths.ToString();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (var player in listPlayerInfo)
        {
            if (player.kills > killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && currentState != GameState.ENDING)
            {
                currentState = GameState.ENDING;

                UpdateGameStateSend((int)currentState);
            }
        }
    }

    private void EndGame()
    {
        currentState = GameState.ENDING;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIManager.ins.endScreen.SetActive(true);
        UIManager.ins.ShowLeaderBoard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = endCamPos.position;
        Camera.main.transform.rotation = endCamPos.rotation;
    }

    private IEnumerator ie_End()
    {
        yield return new WaitForSeconds(waitAfterEnding);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void SetupTimer() 
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UIManager.ins.timerTxt.text = timeToDisplay.Minutes.ToString("00")+":"+timeToDisplay.Seconds.ToString("00");
    }
}

[System.Serializable]
public class PlayerInfo 
{
    public string name;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string name,int actor,int kills,int deaths)
    {
        this.name = name;
        this.actor = actor;
        this.kills = kills;
        this.deaths = deaths;
    }
}

public enum EventType : byte
{
    NEW_PLAYER,
    LIST_PLAYER,
    UPDATE_STAT,
    UPDATE_GAME_STATE,
    TIMER_SYNC
}

public enum GameState
{
    WAITING,
    PLAYING,
    ENDING
}
