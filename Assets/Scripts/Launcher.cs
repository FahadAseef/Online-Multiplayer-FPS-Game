using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    [SerializeField] GameObject LoadingScreen;
    [SerializeField] TMP_Text LoadingText;
    [SerializeField] GameObject MenuButtons;
    [SerializeField] GameObject CreateRoomPanel;
    [SerializeField] TMP_InputField RoomNameInput;
    [SerializeField] GameObject RoomPanel;
    [SerializeField] TMP_Text RoomNameText;
    [SerializeField] GameObject ErrorPanel;
    [SerializeField] TMP_Text ErrorText;
    [SerializeField] GameObject RoomBrowserPanel;
    public RoomButton TheRoomButton;
    private List<RoomButton> AllroomButtons = new List<RoomButton>();
    [SerializeField] TMP_Text PlayerLabel;
    private List<TMP_Text> AllPlayerLabels=new List<TMP_Text>();
    [SerializeField] GameObject NameInputPanel;
    [SerializeField] TMP_InputField PlayerNameInput;
    private bool HasSetNickName;
    public string LevelToPlay;
    [SerializeField] GameObject StartButton;
    [SerializeField] GameObject RoomTestButton;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        CloseMenus();
        LoadingScreen.SetActive(true);
        LoadingText.text = "Conecting To Network...";
        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif
    }
  
    private void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomPanel.SetActive(false);
        RoomPanel.SetActive(false);
        ErrorPanel.SetActive(false);
        RoomBrowserPanel.SetActive(false);
        NameInputPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        LoadingText.text = "Joining Lobby...";
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!HasSetNickName)
        {
            CloseMenus();
            NameInputPanel.SetActive(true);
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                PlayerNameInput.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        else
        {
            PhotonNetwork.NickName= PlayerPrefs.GetString("PlayerName");
        }
    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        CreateRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions Options = new RoomOptions();
            Options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(RoomNameInput.text,Options);
            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        RoomPanel.SetActive(true);
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach(TMP_Text players in AllPlayerLabels)
        {
            Destroy(players.gameObject);
        }
        AllPlayerLabels.Clear();

        Player[] Players = PhotonNetwork.PlayerList;
        for(int i = 0; i < Players.Length; i++)
        {
            TMP_Text NewPlayerLabel = Instantiate(PlayerLabel, PlayerLabel.transform.parent);
            NewPlayerLabel.text = Players[i].NickName;
            NewPlayerLabel.gameObject.SetActive(true);
            AllPlayerLabels.Add(NewPlayerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text NewPlayerLabel = Instantiate(PlayerLabel, PlayerLabel.transform.parent);
        NewPlayerLabel.text = newPlayer.NickName;
        NewPlayerLabel.gameObject.SetActive(true);
        AllPlayerLabels.Add(NewPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();
        ErrorText.text="Failed to create room"+message;
        ErrorPanel.SetActive(true);
    }

    public void ClosErrorPanel()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Loading...";
        LoadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        RoomBrowserPanel.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in AllroomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllroomButtons.Clear();

        TheRoomButton.gameObject.SetActive(false);

        for(int i=0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton NewButton = Instantiate(TheRoomButton,TheRoomButton.transform.parent);
                NewButton.SetButtonDetails(roomList[i]);
                NewButton.gameObject.SetActive(true);
                AllroomButtons.Add(NewButton);
            }
        }

    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        LoadingText.text = "Joining Room...";
        LoadingScreen.SetActive(true);
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(PlayerNameInput.text))
        {
            PhotonNetwork.NickName=PlayerNameInput.text;
            PlayerPrefs.SetString("PlayerName", PlayerNameInput.text);

            CloseMenus();
            MenuButtons.SetActive(true);
            HasSetNickName= true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(LevelToPlay);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void QuickJoin()
    {
        RoomOptions Options = new RoomOptions();
        Options.MaxPlayers = 8;

        PhotonNetwork.CreateRoom("Test");
        CloseMenus();
        LoadingText.text = "Creating Room...";
        LoadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
