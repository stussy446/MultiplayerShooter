using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class MatchManager : MonoBehaviour
{
    private const int STARTING_SCENE_INDEX = 0;
        
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(STARTING_SCENE_INDEX);
        }
    }

}
