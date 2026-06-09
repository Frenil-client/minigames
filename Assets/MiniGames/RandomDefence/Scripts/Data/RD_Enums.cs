namespace MiniGames.RandomDefence
{
    public enum TowerType  { Worrior, Mage, Bowman}
    public enum AttackType { Single, AoE, None }

    public enum CCType     { None, Stun, Slow, ArmorBreak, AttackBuff, AttackSpeedBuff }

    public enum RoundPhase { Prepare, Wave, End }

    public enum RoundType  { Normal, Boss, FinalBoss }

    public enum GameState  { Idle, Playing, Paused, GameOver, Clear }
}
