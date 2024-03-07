using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

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

    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerId);

        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 3.0f);
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("00_MENU");
    }

    [PunRPC]
    public void GiveGloves(int playerId, bool initialGive)
    {
        if (!initialGive)
        {
            GetPlayer(playerWithGloves).HasBoxGlove(false);
        }
        playerWithGloves = playerId;
        GetPlayer(playerId).HasBoxGlove(true);
        glovesPickupTime = Time.time;
    }

    public bool CanGetGloves()
    {
        if (Time.time > glovesPickupTime + invictibleDuration)
        {
            return true;
        }
        else return false;
    }



    void SpawnPlayer()
    {
        GameObject playerObject = 
            PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0,spawnPoints.Length)].position,
            Quaternion.identity);

        PlayerController playerScript = playerObject.GetComponent<PlayerController>();

        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }
    public PlayerController GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }
}
