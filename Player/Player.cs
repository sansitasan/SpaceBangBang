using Cinemachine;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SpaceBangBang
{
    //플레이어 상태
    public enum PlayerStates
    {
        None,
        Idle,
        Move,
        Crouch,
        Dead,
        Hit
    }

    //플레이어가 바라보는 방향
    public enum PlayerLookStates
    {
        None,
        Fst,
        Snd,
        Trd,
        Fth
    }

    public enum ClipType
    {
        FootStep,
        Hit,
        DodgeStart,
        DodgeEnd = 4,
        GunChange,
        CardDraw,
        CardRemove
    }

    public class Player : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        public PlayerStat _stat = null;
        public Gun gun;

        public CardHandle _cardHandle;

        private PlayerState _state = null;
        private PlayerStateHandle _stateHandle = new PlayerStateHandle();

        private PlayerLookState _lookState = null;
        private LookStateHandle _lookHandle = new LookStateHandle();

        [field: SerializeField]
        public Animator Anim { get; private set; }
        [field: SerializeField]
        public SpriteRenderer Spriterenderer { get; private set; }
        [field: SerializeField]
        public Rigidbody2D Rd { get; private set; }
        public PlayerStates PS { get; set; }
        public PlayerLookStates PSLook { get; set; }
        public float Speed { get; set; } = 30;

        public Transform Tplayer;

        public Vector2 Pos { get; private set; }
        public PlayerStat Stat { get => _stat; }
        private float _angle = 0;

        public CardHandlUI _cardHandleUI;

        [field: SerializeField]
        private HPUI _hpUI;

        public CAudio cAudio;
        public AudioClip[] clips;

        public CinemachineVirtualCamera _camera;

        public Action<Player> Clear = null;

        public Collider2D[] PColliders;
        public GameObject Shadow;

        [SerializeField]
        private TextMeshProUGUI _nickname;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            Anim.SetFloat("runspeed", 1);

            PS = PlayerStates.Idle;
            PSLook = PlayerLookStates.Fst;
            _state = _stateHandle.HandleInput(PlayerStates.Idle);
            _lookState = _lookHandle.HandleInput(PlayerLookStates.Fst);
            SelectPlayer((int)photonView.Owner.CustomProperties["Character"]);
            _nickname.text = this.photonView.Owner.NickName;

            if (photonView.IsMine)
            {
                EquipWeapon(WeaponTypes.K2, PhotonNetwork.LocalPlayer);
                LookUpdate(-Tplayer.position);
            }
            //수정사항
            _hpUI.Init(_stat);
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
            NetworkManager.Instance.EndBattle -= PClear;
            NetworkManager.Instance.EndBattle += PClear;
            NetworkManager.Instance.alivePlayers.Add(this);
        }

        [PunRPC]
        public void CardInit()
        {
            _cardHandle.Init(this, _stat.MaxCard, _cardHandleUI);
        }

        public void SelectPlayer(int id)
        {
            _stat = GameManager.Data.PlayerDict[id].Clone();
        }

        //플레이어 상태 변경
        [PunRPC]
        private void HandleInput(PlayerStates ps)
        {
            if (_state == null)
                _state = _stateHandle.HandleInput(PlayerStates.Idle);
            ps = _state.HandleInput(ps, PS);
            PlayerState state = _stateHandle.HandleInput(ps);

            //플레이어 상태가 전과 다를 경우(바뀐 경우)
            if (state != null)
            {
                _state.Exit(this);
                _state = null;
                _state = state;
                _state.Enter(this);
            }
        }

        //플레이어가 바라보는 방향 변경
        [PunRPC]
        private void HandleLookInput(PlayerLookStates ps)
        {
            if (_lookState == null)
                _lookState = _lookHandle.HandleInput(PlayerLookStates.Fst);
            ps = _lookState.HandleInput(ps, PSLook);
            PlayerLookState state = _lookHandle.HandleInput(ps);

            //방향 상태가 전과 다를 경우(바뀐 경우)
            if (state != null)
            {
                _lookState = null;
                _lookState = state;
                _lookState.Enter(this);
            }
        }

        //플레이어 조이스틱이 사용하는 함수 - 드래그 중일 시, 드래그가 끝날 시 
        public void StateUpdate(PlayerStates ps, Vector2 vector)
        {
            if (PS == PlayerStates.Dead)
                return;
            Pos = vector;
            photonView.RPC(nameof(HandleInput), RpcTarget.AllBuffered, ps);
        }

        //무기 조이스틱이 사용하는 함수 - 드래그 중일 시
        public void LookUpdate(Vector2 vector)
        {
            if (PS == PlayerStates.Dead)
                return;

            _angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            if (_angle > 0)
            {
                if (_angle > 90)
                    photonView.RPC(nameof(HandleLookInput), RpcTarget.AllBuffered, PlayerLookStates.Snd);
                else
                    photonView.RPC(nameof(HandleLookInput), RpcTarget.AllBuffered, PlayerLookStates.Fst);
            }

            else
            {
                if (_angle < -90)
                    photonView.RPC(nameof(HandleLookInput), RpcTarget.AllBuffered, PlayerLookStates.Trd);
                else
                    photonView.RPC(nameof(HandleLookInput), RpcTarget.AllBuffered, PlayerLookStates.Fth);
            }

            if (!gun.IsEightDirectionShot) gun.photonView.RPC("RotateGunRPC", RpcTarget.All, _angle);
        }

        public void RemoveCard(int id)
        {
            _cardHandle.photonView.RPC("RemoveCardRPC", RpcTarget.All, id);
        }

        public void UseCard(int id = 0)
        {
            CardType card = _cardHandle.GetFstCard(id);
            switch (card)
            {
                case CardType.Bang:
                    bool bshot = gun.Shot();
                    if (bshot)
                        _cardHandle.UseCard(id);
                    break;
                case CardType.Guard:
                    bool bGuard = CheckCanSetUpGuardObject();
                    if (bGuard)
                        _cardHandle.UseCard(id);
                    break;
                default:
                    _cardHandle.UseCard(id);
                    break;
            }
        }

        public void AddCard(int cnt = 1)
        {
            _cardHandle.Draw(cnt);
        }

        //플레이어 상태에 따른 업데이트
        private void FixedUpdate()
        {
            if (photonView.IsMine)
                _state.Update(this);
        }

        public void EquipWeapon(WeaponTypes weaponType, Photon.Realtime.Player player)
        {
            // 서비스 중계 패턴 (Null 서비스)
            ObjectPoolManager.Instance.photonView.RPC("GetGunRPC", RpcTarget.All, weaponType, player);

            if (gun != null)
            {
                ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.DropGun, gun.transform.position, gun.transform.rotation.eulerAngles, 10f, PhotonNetwork.LocalPlayer);
                ObjectPoolManager.Instance.photonView.RPC("ReturnGunRPC", RpcTarget.All, gun.photonView.ViewID, PhotonNetwork.LocalPlayer);
                cAudio.PlaySound(clips[(int)ClipType.GunChange], Sound.Effect);
            }

            photonView.RPC(nameof(SetGunRPC), RpcTarget.All);
            gun.photonView.RPC("RotateGunRPC", RpcTarget.All, _angle);
        }

        [PunRPC]
        private void SetGunRPC()
        {
            gun = transform.GetChild(4).GetComponent<Gun>();
        }

        [PunRPC]
        private void Recover(int hp)
        {
            _stat.HP += hp;
        }

        public bool addHP(int hp)
        {
            photonView.RPC("Recover", RpcTarget.All, hp);

            return true;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.Sender.TagObject = this;
        }

        [PunRPC]
        private void Damaged()
        {
            _stat.HP -= 1;
            if (_stat.HP <= 0)
            {
                HandleInput(PlayerStates.Dead);
                NetworkManager.Instance.alivePlayers.Remove(this);
                _nickname.text = "";
                _hpUI.gameObject.SetActive(false);
                if (PhotonNetwork.IsMasterClient)
                    NetworkManager.Instance.CheckGameStatus();
            }
            else
                HandleInput(PlayerStates.Hit);
        }

        //같은 카드가 사라진다는 보장이 없음
        [PunRPC]
        private void PassiveCardRemoveOtherClient(CardType card)
        {
            _cardHandle.RemovePassiveCard(card);
        }

        public void Hit()
        {
            CardType card = _cardHandle.FindPassiveCard();
            if (card == CardType.None)
                photonView.RPC("Damaged", RpcTarget.All);
            
            else
            {
                photonView.RPC("PassiveCardRemoveOtherClient", RpcTarget.Others, card);
                ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.DodgeText, transform.position, Vector3.zero, 10f, PhotonNetwork.LocalPlayer);
                HandleInput(PlayerStates.Crouch);
            }

        }

        private void PClear(Photon.Realtime.Player[] winner)
        {
            cAudio.Clear();
            bool bwin = Array.Exists(winner, x => x == this.photonView.Owner);
            if(bwin)
                photonView.RPC(nameof(HandleInput), RpcTarget.AllBuffered, PlayerStates.Idle);
            else
            {
                _stat.HP = 0;
                photonView.RPC(nameof(HandleInput), RpcTarget.AllBuffered, PlayerStates.Dead);
            }
            Clear?.Invoke(this);
            NetworkManager.Instance.EndBattle -= PClear;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (photonView.IsMine)
                if (collision.CompareTag("ItemBox"))
                    AddCard(5);
        }

        private bool CheckCanSetUpGuardObject()
        {
            int layerMask = ((1 << LayerMask.NameToLayer("Gun")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
            layerMask = ~layerMask;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(Mathf.Deg2Rad * _angle), Mathf.Sin(Mathf.Deg2Rad * _angle)), 4, layerMask);
            return !hit;
        }

        public Vector3 SetGuardObjectPos()
        {
            return transform.position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * _angle), Mathf.Sin(Mathf.Deg2Rad * _angle), 0) * 4;
        }
    }
}