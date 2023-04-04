namespace SpaceBangBang
{
    //플레이어가 바라보는 방향을 사분면으로 나눔
    public class PlayerLookState
    {
        public PlayerLookState() { }
        public virtual void Enter(Player player) { }
        public virtual PlayerLookStates HandleInput(PlayerLookStates ps, PlayerLookStates player)
        {
            if (ps == player)
                return PlayerLookStates.None;

            return ps;
        }
    }

    public class LookStateHandle
    {
        private Fst _fst = new Fst();
        private Snd _snd = new Snd();
        private Trd _trd = new Trd();
        private Fth _fth = new Fth();

        public PlayerLookState HandleInput(PlayerLookStates ps)
        {
            //받은 방향정보와 현재 방향정보가 같으면 null
            if (ps == PlayerLookStates.None)
                return null;
            //다르면 해당 state로 교체
            else
            {
                switch (ps)
                {
                    case PlayerLookStates.Fst:
                        return _fst;
                    case PlayerLookStates.Snd:
                        return _snd;
                    case PlayerLookStates.Trd:
                        return _trd;
                    case PlayerLookStates.Fth:
                        return _fth;
                    default:
                        return null;
                }
            }
        }
    }

    public class Fst : PlayerLookState
    {
        public Fst() { }

        public Fst(Player player)
        {
            player.PSLook = PlayerLookStates.Fst;
        }

        public override void Enter(Player player)
        {
            player.Anim.SetBool("lookup", true);
            player.Spriterenderer.flipX = false;
            player.PSLook = PlayerLookStates.Fst;
        }

        public override PlayerLookStates HandleInput(PlayerLookStates ps, PlayerLookStates player)
        {
            return base.HandleInput(ps, player);
        }
    }

    public class Snd : PlayerLookState
    {
        public Snd() { }

        public Snd(Player player)
        {
            player.PSLook = PlayerLookStates.Snd;
        }

        public override void Enter(Player player)
        {
            player.Anim.SetBool("lookup", true);
            player.Spriterenderer.flipX = true;
            player.PSLook = PlayerLookStates.Snd;
        }

        public override PlayerLookStates HandleInput(PlayerLookStates ps, PlayerLookStates player)
        {
            return base.HandleInput(ps, player);
        }
    }

    public class Trd : PlayerLookState
    {
        public Trd() { }

        public Trd(Player player)
        {
            player.PSLook = PlayerLookStates.Trd;
        }

        public override void Enter(Player player)
        {
            player.Anim.SetBool("lookup", false);
            player.Spriterenderer.flipX = true;
            player.PSLook = PlayerLookStates.Trd;
        }

        public override PlayerLookStates HandleInput(PlayerLookStates ps, PlayerLookStates player)
        {
            return base.HandleInput(ps, player);
        }
    }

    public class Fth : PlayerLookState
    {
        public Fth() { }

        public Fth(Player player)
        {
            player.PSLook = PlayerLookStates.Fth;
        }

        public override void Enter(Player player)
        {
            player.Anim.SetBool("lookup", false);
            player.Spriterenderer.flipX = false;
            player.PSLook = PlayerLookStates.Fth;
        }

        public override PlayerLookStates HandleInput(PlayerLookStates ps, PlayerLookStates player)
        {
            return base.HandleInput(ps, player);
        }
    }
}