using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIcontroller : MonoBehaviour
{
    public static UIcontroller instance;
    public TMP_Text OverheatMessagte;
    public Slider WeaponTempSlider;
    public GameObject DeathPanel;
    public TMP_Text DeathText;
    public Slider HealthSlider;
    public TMP_Text healthText;
    public TMP_Text KillsText;
    public TMP_Text DeathsText;
    public GameObject LeaderBoard;
    public LeadeBoard LeaderBoardPlayerDisplay;
    public GameObject EndScreen;
    public TMP_Text timerTect;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        OverheatMessagte.gameObject.SetActive(false);
        DeathPanel.gameObject.SetActive(false);
    }

}
