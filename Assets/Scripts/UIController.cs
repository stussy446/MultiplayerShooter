using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider;
    public GameObject deathScreen;
    public TMP_Text deathText;
    public Slider healthSlider;
    public TMP_Text killsText;
    public TMP_Text deathsText;


    private void Awake()
    {
        instance = this;
    }



}
