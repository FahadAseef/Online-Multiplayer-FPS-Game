using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

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

    public GameObject OptionsScreen;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        OverheatMessagte.gameObject.SetActive(false);
        DeathPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
        if (OptionsScreen.activeInHierarchy && Cursor.lockState!=CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if (!OptionsScreen.activeInHierarchy)
        {
            OptionsScreen.SetActive(true);
        }
        else
        {
            OptionsScreen.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();  
    }

}
