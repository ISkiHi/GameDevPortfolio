/// <summary>
/// Gate
/// 
/// Interactive obstacle that can open or close in response to pressure
/// tile activation. When closing, the gate checks for overlapping entities
/// and reacts based on their traits.
/// 
/// Behaviour:
/// - Vulnerable entities (items or enemies) are destroyed
/// - Non-vulnerable entities block the gate from closing
/// - The player can also be affected if standing on the tile
/// 
/// The gate will not close if blocking entities are present and will
/// retry the interaction after the player's next move.
/// </summary>

namespace DontWitchMeNow
{
    public class Gate : InteractiveItem
    {
        [Header("Popup Text")]
        [SerializeField] private string gateMovementText = "Swoosh!";
        [SerializeField] private string gateBlockedText = "Blocked!";

        private bool playerMovementTracked = false;
        private List<BaseMovingUnit> overlapEntityList;

        private int CheckForBlockingEntities()
        {
            int blockingEntities = 0;
            GenerateOverlapList();

            for (int entity = 0; entity < overlapEntityList.Count; entity++)
            {
                if (overlapEntityList[entity].traits.HasTrait(TraitsList.VULNERABLE))
                {
                    if (overlapEntityList[entity] is Item item)
                        item.TryDestroyItem();
                    else if (overlapEntityList[entity] is EnemyBase enemy)
                        enemy.EnemyHit();
                }
                else
                    blockingEntities++;
            }

            return blockingEntities;
        }

        private void GenerateOverlapList()
        {
            overlapEntityList = new List<BaseMovingUnit>(GameManager.Instance.gridManager.IsObjectOnTile(gridPosition));
            overlapEntityList.Remove(this); // Doesn't include self
        }

        // Gate opens
        protected override void ActivateInteraction()
        {   
            base.ActivateInteraction();

            gateOpen = true;
        }
        
        // Gate closes unless blocked
        protected override void DeactivateInteraction()
        {
            base.DeactivateInteraction();

            if (CheckForBlockingEntities() > 0)
            {
                popupFields.message = gateBlockedText;

                if (!playerMovementTracked)
                {
                    GameManager.Instance.SubscribeGameTrigger(GameTriggers.OnPlayerMoveComplete, DeactivateInteraction);
                    playerMovementTracked = true;
                }   
            }       
            else
            {   
                if (playerMovementTracked)
                {
                    GameManager.Instance.UnsubscribeGameTrigger(GameTriggers.OnPlayerMoveComplete, DeactivateInteraction);
                    playerMovementTracked = false;
                }

                popupFields.message = gateMovementText;

                if (GameManager.Instance.player.gridPosition == gridPosition)
                    itemsManager.PlayerHit(this, true);
            }
        }
    }
}