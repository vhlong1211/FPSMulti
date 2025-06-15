using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : SingletonMonoBehaviour<PlayerSpawner>
{
    public GameObject playerPrefab;
    private GameObject player;
    public GameObject deathEffPrefab;

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.ins.GetRandomPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name,spawnPoint.position,spawnPoint.rotation);

        player.GetComponent<PlayerController>().Setup();
    }

    public void Die(string damager)
    {
        UIManager.ins.killedByTxt.text = "YOU WERE KILLED BY " + damager;
        StartCoroutine(ie_Die());
    }

    public IEnumerator ie_Die()
    {
        PhotonNetwork.Instantiate(deathEffPrefab.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIManager.ins.deathScreen.SetActive(true);
        yield return new WaitForSeconds(5f);
        UIManager.ins.deathScreen.SetActive(false);
        SpawnPlayer();
    }
}
