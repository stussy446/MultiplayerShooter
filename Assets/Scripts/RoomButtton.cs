using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButtton : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;

    private RoomInfo info;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = info.Name;
    }

    public void OpenRoom()
    {
        Launcher.instance.JoinRoom(info);
    }


}
