/// <summary>
/// Game Over Manager
/// 
/// Handles the end-of-level flow, including calculating star rewards,
/// displaying the game over screen, and triggering star animations.
/// 
/// Also manages a player feedback link (playtesting purposes) and stores level completion
/// results through the level data system.
/// </summary>

namespace DontWitchMeNow
{
    public class GameOverManager : StaticInstance<GameOverManager>
    {
        private GameOverUIDocument uiDocument = new GameOverUIDocument();
        
        private readonly int maxStars = 3;
        private readonly string url = "[REDACTED]";

        public int StartGameOverScreen(GameOverState endState)
        {
            bool isWin = GameManager.Instance.IsGameOverWin();
            int starsN = 0;

            if (isWin)
            {
                starsN = GetStarsN();
                LevelDataManager.Instance.SetCurrentLevelStars(starsN);
                StartCoroutine(Stars(starsN));
            }

            return starsN;
        }

        public int GetStarsN()
        {
            int starsN = maxStars - GameManager.Instance.challengeManager.challenges.Count;

            foreach (LevelChallenges challenge in GameManager.Instance.challengeManager.challenges)
            {
                if (challenge.isComplete && !challenge.isFailed)
                    starsN++;
            }

            return starsN;
        }

        private IEnumerator Stars(int starsN)
        {
            for (int i = 0; i < starsN; i++)
            {
                uiDocument.PlayStar(i);
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void FeedbackLink()
        {
            Application.OpenURL(url);
        }
    }
}