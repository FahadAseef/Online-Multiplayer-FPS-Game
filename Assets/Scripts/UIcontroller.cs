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
