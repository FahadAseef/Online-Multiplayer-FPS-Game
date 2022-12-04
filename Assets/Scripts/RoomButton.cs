using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text ButtonText;
    private RoomInfo Info;
    
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        Info= inputInfo;
        ButtonText.text = Info.Name;
    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(Info);
    }

}
