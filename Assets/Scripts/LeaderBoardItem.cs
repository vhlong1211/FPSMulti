using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LeaderBoardItem : MonoBehaviour
{
    public TMP_Text playerNameTxt;
    public TMP_Text killsTxt;
    public TMP_Text deathsTxt;

    public void SetDetails(string name,int kill,int death)
    {
        playerNameTxt.text = name;
        killsTxt.text = kill.ToString();
        deathsTxt.text = death.ToString();
    }

}
