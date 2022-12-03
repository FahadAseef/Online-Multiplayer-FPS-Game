using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeadeBoard : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    public TMP_Text KillsText;
    public TMP_Text DeathsText;

    public void SetDetails(string name,int kills,int deaths)
    {
        PlayerNameText.text= name;
        KillsText.text = kills.ToString();
        DeathsText.text = deaths.ToString();
    }


}
