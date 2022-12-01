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
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
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

    public void NewPlayerSend(string UserName)
    {
        object[] Package = new object[4];
        Package[0] = UserName;
        Package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        Package[2] = 0;
        Package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            Package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );

    }

    public void NewPlayerRecieve(object[] DataRecieved)
    {
        PlayerInfo Player = new PlayerInfo((string) DataRecieved[0],(int) DataRecieved[1],(int) DataRecieved[2],(int) DataRecieved[3]);
        AllPlayers.Add(Player);
        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[AllPlayers.Count];
        for(int i= 0; i < AllPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = AllPlayers[i].Name;
            piece[1] = AllPlayers[i].Actor;
            piece[2] = AllPlayers[i].Kills;
            piece[3] = AllPlayers[i].Death;
            package[i] = piece;
        }
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayerRecieve(object[] DataRecieved)
    {
        AllPlayers.Clear();
        for(int i= 0; i < DataRecieved.Length; i++)
        {
            object[] piece = (object[])DataRecieved[i];
            PlayerInfo player = new PlayerInfo(
                (String)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            AllPlayers.Add(player);
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.Actor)
            {
                Index = i;
            }
        }
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
