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

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else 
        {
            NewPlayerSend(PhotonNetwork.NickName);
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

        for (int i = 0; i < dataReceived.Length;i++)
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

    public void UpdateStatSend(int actor,int type,int amountChange)
    {
        object[] package = new object[3] {actor,type,amountChange };

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

                if(i == index)
                     UpdateStatDisplay();
            }
        }
    }

    public void UpdateStatDisplay()
    {
        UIManager.ins.killsTxt.text = "Kills: "+ listPlayerInfo[index].kills.ToString();
        UIManager.ins.deathsTxt.text = "Deaths: " + listPlayerInfo[index].deaths.ToString();
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
    UPDATE_STAT
}
