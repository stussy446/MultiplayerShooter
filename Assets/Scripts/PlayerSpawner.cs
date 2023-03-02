using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject deathEffect;

    [Header("Spawn delay in seconds")]
    [SerializeField] float respawnDelay = 5f;

    private GameObject player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        UIController.instance.healthSlider.value = player.GetComponent<PlayerController>().maxHealth;
    }

    public void Die(string damager)
    {
        UIController.instance.deathText.text = $"You were killed by {damager}";
        MatchManager.instance.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (player != null)
        {
            StartCoroutine(DieCo());
        }

    }

    private IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnDelay);

        UIController.instance.deathScreen.SetActive(false);
        SpawnPlayer();
    }
}
