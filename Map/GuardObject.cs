using Photon.Pun;
using SpaceBangBang;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardObject : MonoBehaviourPun
{
    [SerializeField]
    private int _maxHP;
    private int _currentHP;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private Coroutine _returnCoroutine;
    public Coroutine ReturnCoroutine { get => _returnCoroutine; set => _returnCoroutine = value; }
    public SpriteRenderer SpriteRenderer { get => _spriteRenderer; }
    public int CurrentHP { get => _currentHP;}

    private Vector3 _startPos;

    [SerializeField]
    private Sprite[] sprites; 

    public void Init(Vector3 startPos, Photon.Realtime.Player player)
    {
        _startPos = startPos;
        _currentHP = _maxHP;
        _spriteRenderer.color = Color.green;
        _spriteRenderer.sprite = sprites[(int)player.CustomProperties["Character"]];
    }

    private void OnEnable()
    {
        transform.position = _startPos;
    }

    [PunRPC]
    public void HitRPC()
    {
        _currentHP -= 1;

        switch (_currentHP)
        {
            case 2:
                _spriteRenderer.color = Color.yellow;
                break;
            case 1:
                _spriteRenderer.color = Color.red;
                break;
        }

        if (_currentHP <= 0 && photonView.IsMine)
        {
            ObjectPoolManager.Instance.photonView.RPC("ReturnGuardObjectRPC", RpcTarget.AllBuffered, photonView.ViewID, PhotonNetwork.LocalPlayer);
        }
    }
}
