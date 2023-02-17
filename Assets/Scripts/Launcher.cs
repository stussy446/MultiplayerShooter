using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [Header("Menu Configs")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject menuButtons;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject createRoomScreen;
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] GameObject roomScreen;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameObject errorScreen;
    [SerializeField] TMP_Text errorText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CloseMenus();
        OpenLoadingMenu();

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        OpenMenuButtons();
    }

    private void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
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

}
