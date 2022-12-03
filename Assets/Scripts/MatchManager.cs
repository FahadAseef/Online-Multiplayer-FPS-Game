using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
//using System;

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
    private List<LeadeBoard> leaderboardplayers = new List<LeadeBoard>();


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
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIcontroller.instance.LeaderBoard.activeInHierarchy)
            {
                UIcontroller.instance.LeaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
        
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("recieved event " + theEvent);            

            switch (theEvent)
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

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );

    }

    public void NewPlayerRecieve(object[] dataRecieved)
    {
        PlayerInfo player = new PlayerInfo((string) dataRecieved[0],(int) dataRecieved[1],(int) dataRecieved[2],(int) dataRecieved[3]);
        AllPlayers.Add(player);
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
                (string)piece[0],
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

    public void UpdateStatSend(int actorsending,int statetoupdate,int amounttochange)
    {
        object[] package=new object[] {actorsending,statetoupdate,amounttochange};
        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.UpdateStat,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void UpdateStatRecieve(object[] DataRecieved)
    {
        int actor = (int)DataRecieved[0];
        int stattype = (int)DataRecieved[1];
        int amount = (int)DataRecieved[2];

        for(int i = 0; i < AllPlayers.Count; i++)
        {
            if (AllPlayers[i].Actor == actor)
            {
                switch (stattype)
                {
                    case 0: //kills
                        AllPlayers[i].Kills += amount;
                        Debug.Log("Player " + AllPlayers[i].Name + " : kills " + AllPlayers[i].Kills);
                        break;
                    case 1: //deaths
                        AllPlayers[i].Death += amount;
                        Debug.Log("Player " + AllPlayers[i].Name + " : deaths " + AllPlayers[i].Death);
                        break;
                }
                if (i == Index)
                {
                    UpdateStatsDisplay();
                }
                if (UIcontroller.instance.LeaderBoard.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }

                break;
            }
        }
    }

    public void UpdateStatsDisplay()
    {
        if (AllPlayers.Count > Index)
        {
            UIcontroller.instance.KillsText.text = "kills : " + AllPlayers[Index].Kills;
            UIcontroller.instance.DeathsText.text = "Deaths : " + AllPlayers[Index].Death;
        }
        else
        {
            UIcontroller.instance.KillsText.text = "kills : 0";
            UIcontroller.instance.DeathsText.text = "Deaths : 0";
        }
    }

    void ShowLeaderBoard()
    {
        UIcontroller.instance.LeaderBoard.SetActive(true);
        foreach(LeadeBoard lp in leaderboardplayers)
        {
            Destroy(lp.gameObject);
        }
        leaderboardplayers.Clear();
        UIcontroller.instance.LeaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = sortPlayer(AllPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeadeBoard newPlayerDisplay = Instantiate(UIcontroller.instance.LeaderBoardPlayerDisplay, UIcontroller.instance.LeaderBoardPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(player.Name, player.Kills, player.Death);
            newPlayerDisplay.gameObject.SetActive(true);
            leaderboardplayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> sortPlayer(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();
        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];
            foreach (PlayerInfo player in players)
            {
                if (sorted.Contains(player))
                {
                    if (player.Kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.Kills;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }

        return sorted;
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
