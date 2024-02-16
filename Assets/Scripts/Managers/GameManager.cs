using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;
    public float timeToRound;
    public float invictibleDuration;

    private float glovesPickupTime;

    [Header("Player")]
    public string playerPrefabLocation;

    public Transform[] spawnPoints;
    public PlayerController[] players;
    public int playerWithGloves;
    private int playersInGame;

    public static GameManager instance;

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("AmI_InGame", RpcTarget.All);
    }

    [PunRPC]
    void AmI_InGame()
    {
        playersInGame++;

        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        GameObject playerObject = 
            PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0,spawnPoints.Length)].position,
            Quaternion.identity);

        PlayerController playerScript = playerObject.GetComponent<PlayerController>();

        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
