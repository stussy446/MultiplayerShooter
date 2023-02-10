using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject menuButtons;
    [SerializeField] TMP_Text loadingText;

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
}
