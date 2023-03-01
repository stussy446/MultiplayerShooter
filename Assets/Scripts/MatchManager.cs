using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const int STARTING_SCENE_INDEX = 0;
        
    public static MatchManager instance;

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(STARTING_SCENE_INDEX);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatReceive(data);
                    break;
                default:
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

    public void NewPlayerReceive(object[] dataReceived)
    {

    }

    public void ListPlayersSend()
    {

    }

    public void ListPlayersReceive(object[] dataReceived)
    {

    }

    public void UpdateStatSend()
    {

    }

    public void UpdateStatReceive(object[] dataReceived)
    {

    }
}


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        this.name = _name;
        this.actor = _actor;
        this.kills = _kills;
        this.deaths = _deaths;
    }
}
