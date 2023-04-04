using DG.Tweening;
using UnityEngine;

namespace SpaceBangBang
{
    //플레이어의 상태를 나눔
    public class PlayerState
    {
        public PlayerState() { }

        public PlayerState(Player player)
        {

        }
        public virtual PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            if (ps == player)
                return PlayerStates.None;
            //다를 경우 받은 상태로 state 전환
            return ps;
        }
        public virtual void Enter(Player player) { }
        public virtual void Exit(Player player) { }
        public virtual void Update(Player player) { }
    }

    public class PlayerStateHandle
    {
        private Idle _idle = new Idle();
        private Move _move = new Move();
        private Hit _hit = new Hit();
        private Crouch _crouch = new Crouch();
        private Dead _dead = new Dead();

        public PlayerState HandleInput(PlayerStates ps)
        {
            if (ps == PlayerStates.None)
                return null;
            //다를 경우 받은 상태로 state 전환
            else
            {
                switch (ps)
                {
                    case PlayerStates.Move:
                        return _move;
                    case PlayerStates.Idle:
                        return _idle;
                    case PlayerStates.Crouch:
                        return _crouch;
                    case PlayerStates.Dead:
                        return _dead;
                    case PlayerStates.Hit:
                        return _hit;
                    default:
                        return null;
                }
            }
        }
    }

    public class Move : PlayerState
    {
        public Move()
        {
        }

        public Move(Player player)
        {
            player.PS = PlayerStates.Move;
        }

        public override void Enter(Player player)
        {
            player.PS = PlayerStates.Move;
            player.Anim.SetBool("moving", true);
            player.cAudio.PlaySound(player.clips[(int)ClipType.FootStep], Sound.Bgm, 1.5f);
        }

        public override void Exit(Player player)
        {
            player.Anim.SetBool("moving", false);
            player.Rd.velocity = Vector2.zero;
            player.cAudio.StopSound();
        }

        public override void Update(Player player)
        {
            player.Rd.MovePosition(player.transform.position + new Vector3(Mathf.Lerp(0, player.Pos.x, 0.8f),
                Mathf.Lerp(0, player.Pos.y, 0.8f)) * Time.deltaTime * player.Speed);
        }

        public override PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            return base.HandleInput(ps, player);
        }
    }

    public class Idle : PlayerState
    {
        public Idle()
        {
        }

        public Idle(Player player)
        {
            player.PS = PlayerStates.Idle;
        }

        public override void Enter(Player player)
        {
            player.PS = PlayerStates.Idle;
        }

        public override void Exit(Player player) { }

        public override void Update(Player player) { }

        public override PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            return base.HandleInput(ps, player);
        }
    }

    public class Hit : PlayerState
    {
        private bool _bcan;
        private Sequence _seq;
        public Hit()
        {
        }

        public Hit(Player player)
        {
            player.PS = PlayerStates.Hit;
        }

        public override void Enter(Player player)
        {
            _bcan = false;
            player.PS = PlayerStates.Hit;
            player.cAudio.PlaySound(player.clips[(int)ClipType.Hit], Sound.Effect);
            player.Anim.SetBool("hit", true);
            if (_seq != null)
                _seq.Restart();

            else
                _seq = DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        _bcan = true;
                        player.StateUpdate(PlayerStates.Idle, Vector2.zero);
                    }).SetAutoKill(false);
        }

        public override void Exit(Player player)
        {
            player.Anim.SetBool("hit", false);
        }

        public override void Update(Player player)
        {
        }

        public override PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            if ((ps != PlayerStates.Dead && ps != PlayerStates.Crouch) && !_bcan)
                return PlayerStates.None;
            if (ps == PlayerStates.Dead || ps == PlayerStates.Crouch)
                _seq.Pause();
            return base.HandleInput(ps, player);
        }
    }

    public class Crouch : PlayerState
    {
        private bool _bcan;
        private Sequence _seq;
        public Crouch() { }

        public Crouch(Player player)
        {
            player.PS = PlayerStates.Crouch;
        }

        public override void Enter(Player player)
        {
            _bcan = false;
            player.cAudio.PlaySound(player.clips[Random.Range((int)ClipType.DodgeStart, (int)ClipType.DodgeEnd + 1)], Sound.Effect);
            player.PS = PlayerStates.Crouch;
            player.Anim.SetBool("crouch", true);
            if (_seq != null)
                _seq.Restart();

            else
                _seq = DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        _bcan = true;
                        player.StateUpdate(PlayerStates.Idle, Vector2.zero);
                    }).SetAutoKill(false);
        }

        public override void Exit(Player player)
        {
            player.Anim.SetBool("crouch", false);
        }

        public override void Update(Player player)
        {
        }

        public override PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            if ((ps != PlayerStates.Dead && ps != PlayerStates.Hit) && !_bcan)
                return PlayerStates.None;
            if (ps == PlayerStates.Dead || ps == PlayerStates.Hit)
                _seq.Pause();
            return base.HandleInput(ps, player);
        }
    }

    public class Dead : PlayerState
    {
        public Dead() { }

        public Dead(Player player)
        {
            player.PS = PlayerStates.Dead;
        }

        public override void Enter(Player player)
        {
            player.PS = PlayerStates.Dead;
            player.Shadow.SetActive(false);
            for (int i = 0; i < player.PColliders.Length; ++i)
                player.PColliders[i].enabled = false;
            player.Anim.SetBool("hit", true);
            player.cAudio.PlaySound(player.clips[(int)ClipType.Hit], Sound.Effect);
            player.Clear?.Invoke(player);
            DOTween.Sequence()
                .Append(player.transform.DORotate(new Vector3(0, 0, 90), 1f))
                .AppendCallback(() => player.cAudio.Clear());
        }

        public override void Exit(Player player) { }

        public override void Update(Player player) { }

        public override PlayerStates HandleInput(PlayerStates ps, PlayerStates player)
        {
            return PlayerStates.None;
        }
    }
}