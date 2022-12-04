using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
//using System;
//using System;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }


    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat,
        NextMatch,
        TimerSync
    }
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    private int Index;
    //public EventCodes theEvent;
    private List<LeadeBoard> leaderboardplayers = new List<LeadeBoard>();

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 3;
    public Transform MapCamPoint;
    public GameState state = GameState.Waiting;
    public float WaitAfterEnding = 5;

    public bool Perpetual;

    public float matchLength=180f;
    private float currentMatchTime;
    private float sendTimer;



    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetupTimer();
            if (!PhotonNetwork.IsMasterClient)
            {
                UIcontroller.instance.timerTect.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
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
      
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;

                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;

                    state = GameState.Ending;

                    ListPlayerSend();

                    StateCheck();
                }

                updateTimerDisplay();

                sendTimer -= Time.time;
                if (sendTimer <= 0)
                {
                    sendTimer += 1f;
                    TimerSend();
                }             
            }




        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

           // Debug.Log("recieved event " + theEvent);

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
                case EventCodes.NextMatch:
                    NextMatchRecieve();
                    break;
                case EventCodes.TimerSync:
                    TimerRecieve(data);
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
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);
        AllPlayers.Add(player);
        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[AllPlayers.Count + 1];
        package[0] = state;
        for (int i = 0; i < AllPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = AllPlayers[i].Name;
            piece[1] = AllPlayers[i].Actor;
            piece[2] = AllPlayers[i].Kills;
            piece[3] = AllPlayers[i].Death;
            package[i+1] = piece;
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

        state=(GameState)DataRecieved[0]; 

        for (int i = 1; i < DataRecieved.Length; i++)
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
                Index = i-1;
            }
        }
        StateCheck();
    }

    public void UpdateStatSend(int actorsending, int statetoupdate, int amounttochange)
    {
        object[] package = new object[] { actorsending, statetoupdate, amounttochange };
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

        for (int i = 0; i < AllPlayers.Count; i++)
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
        ScoreCheck();
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
        foreach (LeadeBoard lp in leaderboardplayers)
        {
            Destroy(lp.gameObject);
        }
        leaderboardplayers.Clear();
        UIcontroller.instance.LeaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = sortPlayer(AllPlayers);

        foreach (PlayerInfo player in sorted)
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

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    void ScoreCheck()
    {
        bool WinnerFound = false;
        foreach(PlayerInfo player in AllPlayers)
        {
            if (player.Kills >= killsToWin && killsToWin>0)
            {
                WinnerFound = true;
                break;
            }
        }
        if (WinnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayerSend();
            }
        }
    }

    void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        state = GameState.Ending;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        UIcontroller.instance.EndScreen.SetActive(true);
        ShowLeaderBoard();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = MapCamPoint.position;
        Camera.main.transform.rotation = MapCamPoint.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(WaitAfterEnding);
        if (!Perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!Launcher.Instance.changeMapsBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newlevel=Random.Range(0, Launcher.Instance.allMaps.Length);
                    if (Launcher.Instance.allMaps[newlevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.Instance.allMaps[newlevel]);
                    }
                }
            }
        }
    }

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
         (byte)EventCodes.NextMatch,
         null,
         new RaiseEventOptions { Receivers = ReceiverGroup.All },
         new SendOptions { Reliability = true }
         );
    }

    public void NextMatchRecieve()
    {
        state = GameState.Playing;
        UIcontroller.instance.EndScreen.SetActive(false);
        UIcontroller.instance.LeaderBoard.SetActive(false);

        foreach(PlayerInfo player in AllPlayers)
        {
            player.Kills = 0;
            player.Death = 0;
        }

        UpdateStatsDisplay();
        PlayerSpawner.Instance.SpawnPlayerFN();
        SetupTimer();
    }

    public void SetupTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            updateTimerDisplay();
        }
    }

    public void updateTimerDisplay()
    {
        var timeToDisplay=System.TimeSpan.FromSeconds(currentMatchTime);
        UIcontroller.instance.timerTect.text=timeToDisplay.Minutes.ToString("00")+":" +timeToDisplay.Seconds.ToString("00") ;
    }

    public void TimerSend()
    {
        object[] package = new object[] {(int)currentMatchTime,state};

        PhotonNetwork.RaiseEvent(
         (byte)EventCodes.TimerSync,
         package,
         new RaiseEventOptions { Receivers = ReceiverGroup.All },
         new SendOptions { Reliability = true }
         );
    }

    public void TimerRecieve(object[] dataRecieved)
    {
        currentMatchTime = (int)dataRecieved[0];
        state = (GameState)dataRecieved[1];
        updateTimerDisplay() ;  
        UIcontroller.instance.timerTect.gameObject.SetActive(true);
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
