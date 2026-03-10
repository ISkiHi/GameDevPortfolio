/// <summary>
/// Pressure Tile
/// 
/// Trigger that activates or deactivates connected
/// interactive items when stepped on by a unit.
/// 
/// The tile coordinates interactions between multiple linked objects
/// and manages camera panning so the player can see the triggered
/// mechanisms.
/// 
/// Responsibilities:
/// - Detect unit activation
/// - Trigger linked interactive items
/// - Manage camera panning for interactions
/// - Ensure interactions occur when the game state is ready
/// </summary>

namespace DontWitchMeNow
{
    public class PressureTile : Item
    {
        private PressureObject pressureObject;
        private List<Item> interactiveItems;
        
        public override bool ItemTriggered(LEView unity, Vector2Int dir)
        {
            AsyncItemTriggered(unit, dir);

            return true;
        }

        private async void AsyncItemTriggered(LEView unit, Vector2Int dir)
        {
            while (GameManager.Instance.IsEventBusy())
                await Task.Yield();

            if (GameManager.Instance.player.traits.HasTrait(TraitsList.TIMESTOP))
                return;

            if (pressureObject.objectPressing == unit)
                return;

            ActivateMechanism(true, unit, dir);
        }

        private IEnumerator StartInteractions(bool activate)
        {               
            int itemsToPanTo = 0;
            interactiveItems.Insert(0, this);

            foreach (Item item in interactiveItems)
            {
                isInteracting = true;

                if (item is InteractiveItem interactiveItem)
                {   
                    if (interactiveItem.isHidden)
                    {
                        isInteracting = false;
                        interactiveItem.StartInteraction(activate);

                        continue;
                    }

                    if (!interactiveItem.hasPanned[0] || !interactiveItem.hasPanned[1])
                    {
                        GameManager.Instance.RepositionCamera(item.gridPosition, false, () => InteractionCallback(item, activate));
                        interactiveItem.SetHasPanned(isPressing);
                        itemsToPanTo++;
                    }
                    else
                        InteractionCallback(item, activate);
                }
                else
                    InteractionCallback(item, activate);

                yield return new WaitUntil(() => !isInteracting);
            }
            
            if (itemsToPanTo > 0)
                GameManager.Instance.LoadCameraPosition();

            ItemComplete();
        }
    }
}