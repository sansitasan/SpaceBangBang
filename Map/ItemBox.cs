using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using SpaceBangBang;

public class ItemBox : MonoBehaviourPun
{
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private BattleScene battleScene;

    private void Update()
    {
        if (gameObject.activeSelf && !GameManager.Instance.isEndGame)
        {
            battleScene.UpdateItemBoxUI(transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.GetPhotonView().IsMine)
        {
            photonView.RPC(nameof(SwitchActivateItemBox), RpcTarget.All);
        }
    }

    [PunRPC]
    private void SwitchActivateItemBox()
    {
        bool activeSelf = gameObject.activeSelf;

        gameObject.SetActive(!gameObject.activeSelf);
        if (activeSelf)
        {
            Invoke(nameof(SwitchActivateItemBox), 20f);
            battleScene.SwitchItemBoxUI(false);
        }
        else
        {
            battleScene.SwitchItemBoxUI(true);
        }
    }

    [PunRPC]
    private void SetItemBoxPos(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnDisable()
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(SetItemBoxPos), RpcTarget.All, spawnPoints[Random.Range(0, spawnPoints.Length)].position);
        }
    }
}
