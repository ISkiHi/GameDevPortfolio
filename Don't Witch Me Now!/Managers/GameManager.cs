/// <summary>
/// Game Manager
/// 
/// Central controller for gameplay state. Manages level initialization,
/// move tracking, challenge checks, game over conditions, and more.
/// 
/// This system coordinates communication between gameplay systems
/// such as UI, challenge management, and level data.
/// 
/// Responsibilities:
/// - Initialize and load levels
/// - Track player move count
/// - Handle game over conditions
/// - Trigger analytics events
/// - Coordinate gameplay systems
/// </summary>

namespace DontWitchMeNow
{
    public class GameManager : StaticInstance<GameManager>
    {
        public GameState currentGameState { get; private set; }
        public GameOverState gameOverCause { get; private set; }
        public ChallengeManager challengeManager { get; private set; } = new ChallengeManager();
        public int moveCounter { get; private set; }
        
        private int maxMoves = 0;
        private bool killToWin = true;
        
        public async void Init(bool nextLevel = false)
        {
            moveCounter = 0;

            // Loads the level
            await LoadLevel(nextLevel);

            maxMoves = player.maxMoves; 
        }
        
        public void UpdateMoveCounter()
        {
            moveCounter++;
            UIManager.Instance.UpdateSteps(maxMoves - moveCounter);
            
            if (maxMoves > 0 && moveCounter >= maxMoves && gameOverCause != GameOverState.KilledAll)
                TryStartGameOver(GameOverState.MaxSteps);
        }

        public void TryStartGameOver(GameOverState endState)
        {
            if (endState == GameOverState.KilledAll && !killToWin)
                return;

            if (currentGameState != GameState.GameOver)
                StartCoroutine(StartGameOver(endState));
            else
                gameOverCause = endState;
        }

        public IEnumerator StartGameOver(GameOverState endState)
        {
            gameOverCause = endState;

            yield return new WaitUntil(() => !IsEventBusy() && !HasActiveEvent(player.gameObject));
            int stars = GameOverManager.Instance.StartGameOverScreen(endState);
        }
    }
}