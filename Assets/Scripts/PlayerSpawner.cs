using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    public GameObject PlayerPrefab;
    private GameObject Player;
    public GameObject DeathEffect;

    private void Awake()
    {
        Instance= this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayerFN();
        }
    }

    public void SpawnPlayerFN()
    {
        Transform SpawnPoint = SpawnManager.instance.GetSpawnPoints();
        Player = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoint.position, SpawnPoint.rotation);
    }

    public void DieFN()
    {
        PhotonNetwork.Instantiate(DeathEffect.name,Player.transform.position,Quaternion.identity);
        PhotonNetwork.Destroy(Player);
        SpawnPlayerFN();
    }

}
