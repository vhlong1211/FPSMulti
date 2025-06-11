using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text roomNameTxt;

    private RoomInfo info;

    public void SetButtonDetails(RoomInfo info)
    {
        this.info = info;
        roomNameTxt.text = info.Name;
    }
    public void OnClickOpenRoom()
    {
        Launcher.ins.JoinRoom(info);
    }
}
