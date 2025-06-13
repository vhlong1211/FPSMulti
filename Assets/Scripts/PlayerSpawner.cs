using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : SingletonMonoBehaviour<PlayerSpawner>
{
    public GameObject playerPrefab;
    private GameObject player;
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
}
