/// <summary>
/// Spell Book Collectable
/// 
/// A collectable item used for challenge completion. When collected
/// by the player, the spell book contributes to level objectives and triggers
/// the associated collection animation.
/// 
/// Responsibilities:
/// - Detect player interaction
/// - Trigger challenge progress checks
/// - Play collection animation and remove the item
/// </summary>

namespace DontWitchMeNow
{
    public class SpellBook : Item
    {
        protected override void InitTraits()
        {
            base.InitTraits();

            traits.AddTraits(TraitsList.ITEM, TraitsList.TERRAIN);
        }

        public override bool ItemEffect(LEView unit = null, Vector2Int? dir = null)
        {
            base.ItemEffect(unit, dir);

            if (unit is Player)
            {
                GameManager.Instance.challengeManager.CheckChallenges(false, ChallengeType.CollectSpellBook, this);
                animator.SetTrigger("Activate");
            }
            else
                ItemComplete();

            return true;
        }

        public void AnimationComplete()
        {
            ItemComplete();
            TryDestroyItem();
        }
    }
}