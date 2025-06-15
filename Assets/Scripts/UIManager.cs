using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    public TMP_Text overheatedTxt;
    public Slider weaponTempSlide;
    public Slider playerHealthSlide;

    public GameObject deathScreen;
    public TMP_Text killedByTxt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
