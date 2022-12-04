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
    [SerializeField] float RespawnTime=3f;

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

    public void DieFN(string Damager)
    {       
        UIcontroller.instance.DeathText.text = "You were killed by " + Damager;
        MatchManager.instance.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);  
        if (Player != null)
        {
            StartCoroutine(DieCo());
        }
        //PhotonNetwork.Destroy(Player);
        //SpawnPlayerFN();
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(DeathEffect.name, Player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(Player);
        Player = null;
        UIcontroller.instance.DeathPanel.SetActive(true);
        yield return new WaitForSeconds(RespawnTime);
        UIcontroller.instance.DeathPanel.SetActive(false);
        if (MatchManager.instance.state == MatchManager.GameState.Playing && Player == null)
        {
            SpawnPlayerFN();
        }
    }

}
