using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    private const string PLAYER_NICKNAME = "PlayerNickname";

    public static Launcher instance;
    public string LevelToPlay;

    [Header("Menu Configs")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject menuButtons;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject createRoomScreen;
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] GameObject roomScreen;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerNameLabel;
    [SerializeField] GameObject errorScreen;
    [SerializeField] TMP_Text errorText;
    [SerializeField] GameObject startButton;

    [Header("Room Screen Configs")]
    [SerializeField] GameObject roomBrowserScreen;
    [SerializeField] RoomButtton roomButtton;
    [SerializeField] GameObject testRoomButton;

    [Header("Name Input Screen Configs")]
    [SerializeField] GameObject nameInputScreen;
    [SerializeField] TMP_InputField nameInput;

    private List<RoomButtton> allRoomButtons = new List<RoomButtton>();
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    private bool hasSetNickname;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CloseMenus();
        OpenLoadingMenu();

        PhotonNetwork.ConnectUsingSettings();
#if UNITY_EDITOR
        testRoomButton.SetActive(true);
#endif
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        OpenMenuButtons();

        if (!hasSetNickname)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey(PLAYER_NICKNAME))
            {
                nameInput.text = PlayerPrefs.GetString(PLAYER_NICKNAME);
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString(PLAYER_NICKNAME);
        }
    }

    private void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    private void OpenLoadingMenu()
    {
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";
    }

    private void OpenMenuButtons()
    {
        menuButtons.SetActive(true);

    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text)) 
        {
            RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 8};

            PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);

            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        ToggleStartGameButton();
    }

    private void ToggleStartGameButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text playerName in allPlayerNames)
        {
            Destroy(playerName.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            AddPlayerToRoomList(players[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerToRoomList(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    private void AddPlayerToRoomList(Player player )
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = player.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = $"failed to create room: {message}";
        CloseMenus();
        errorScreen.SetActive(true);

    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room...";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButtton roomButtton in allRoomButtons)
        {
            Destroy(roomButtton.gameObject);
        }

        allRoomButtons.Clear();
        roomButtton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButtton newRoomButton = Instantiate(roomButtton, roomButtton.transform.parent);
                newRoomButton.SetButtonDetails(roomList[i]);
                newRoomButton.gameObject.SetActive(true);

                allRoomButtons.Add(newRoomButton);
            }
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        CloseMenus();
        loadingText.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString(PLAYER_NICKNAME, nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);
            hasSetNickname = true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(LevelToPlay);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        ToggleStartGameButton();
    }

    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 8 };

        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        loadingText.text = "Creating room...";
        loadingScreen.SetActive(true);
    }
}
