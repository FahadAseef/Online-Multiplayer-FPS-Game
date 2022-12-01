using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class MatchManager : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public static MatchManager instance;
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    private int Index;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void Update()
    {
       
    }

    public void OnEvent(EventData PhotonEvent)
    {
        if (PhotonEvent.Code < 200)
        {
            EventCodes TheEvent = (EventCodes)PhotonEvent.Code;
            object[] data = (object[])PhotonEvent.CustomData;

            switch (TheEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecieve(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayerRecieve(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatRecieve(data);
                    break;

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

    public void NewPlayerSend()
    {

    }

    public void NewPlayerRecieve(object[] DataRecieved)
    {

    }

    public void ListPlayerSend()
    {

    }

    public void ListPlayerRecieve(object[] DataRecieved)
    {

    }

    public void UpdateStatSend()
    {

    }

    public void UpdateStatRecieve(object[] DataRecieved)
    {

    }

}

[System.Serializable]
public class PlayerInfo
{
    public string Name;
    public int Actor;
    public int Kills;
    public int Death;

    public PlayerInfo(string _name, int _actor, int _kills, int _death)
    {
        _name = Name;
        _actor = Actor;
        _kills = Kills;
        _death = Death;
    }

}
