namespace MiniGames.Main
{
    /// <summary>
    /// 모든 미니게임 루트 컴포넌트가 구현해야 하는 인터페이스.
    /// MiniGameLauncher 가 생명주기를 제어한다.
    /// </summary>
    public interface IMiniGame
    {
        void OnMiniGameStart();
        void OnMiniGameExit();
    }
}
