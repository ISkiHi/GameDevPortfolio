/// <summary>
/// GuardDog Enemy
/// 
/// Extends PatrolBase behaviour with additional logic to detect when
/// multiple guard dogs occupy the same grid tile. When overlap occurs,
/// a UI counter is displayed showing the number of dogs stacked on the tile.
/// 
/// Responsibilities:
/// - Perform standard patrol behaviour
/// - Detect overlapping guard dogs on the same grid position
/// - Update overlap UI and trigger animations when the count changes
/// </summary>

namespace DontWitchMeNow
{
    public class GuardDog : PatrolBase
    {
        [SerializeField] private GameObject dogOverlapCounterParent;
        [SerializeField] private TextMeshPro dogOverlapCounterText;
        private Animator counterAnimator;
        private int lastDogOverlapAmount = 0;

        public override void StartMove(float delay = 0)
        {
            delay = 0.08f;

            base.StartMove(delay);

            dogOverlapCounterText.enabled = false;
        }

        private void CheckGuardDogOverlap()
        {
            int dogOverlapCounter = 0;

            List<EnemyBase> enemyList = GameManager.Instance.GetEnemyList();

            for (int enemy = 0; enemy < enemyList.Count; enemy++)
            {
                if (enemyList[enemy].gameObject == gameObject) 
                    continue;
                if (enemyList[enemy] is GuardDog)
                    if (newGridPos == enemyList[enemy].newGridPos) // newGridPos derived from PatrolBase
                        dogOverlapCounter++;
            }

            UpdateDogOverlapUI(dogOverlapCounter);
        }
        
        private void UpdateDogOverlapUI(int dogsOverlapped = 0)
        {
            int totalDogsHere = dogsOverlapped + 1;
            dogOverlapCounterParent.SetActive(totalDogsHere > 1);
            
            if (totalDogsHere < 2)
            {
                lastDogOverlapAmount = 0;
                return;
            }

            dogOverlapCounterText.text = totalDogsHere.ToString();  

            if (lastDogOverlapAmount == 0)
            {
                lastDogOverlapAmount = totalDogsHere;
                return;  
            }      
            
            if (lastDogOverlapAmount != totalDogsHere)
            {
                counterAnimator.SetTrigger("CounterChange");
                lastDogOverlapAmount = totalDogsHere;  
            }           
        }
    }
}