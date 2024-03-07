using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector] public int id;

    [Header("Informations")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject gloves;

    [HideInInspector]
    public float curTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(curTime >= GameManager.instance.timeToRound
                && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;

                GameManager.instance.photonView.RPC("WinGame" ,RpcTarget.All,id);
            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            if (gloves.activeInHierarchy)
                curTime += Time.deltaTime;
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void HasBoxGlove(bool hasGlove)
    {
        gloves.SetActive(hasGlove);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithGloves)
            {
                if (GameManager.instance.CanGetGloves())
                {
                    GameManager.instance.photonView.RPC("GiveGloves", RpcTarget.All, id, false);
                }
            }
        }
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        if(id == 1)
        {
            GameManager.instance.GiveGloves(id, true);
        }

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            rig.isKinematic = true;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

        if (stream.IsWriting) { stream.SendNext(curTime); }
        else if(stream.IsReading){
            curTime = (float)stream.ReceiveNext();
        }
    }
}
