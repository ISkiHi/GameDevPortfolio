/// <summary>
/// Invisibility Cloak Power
/// 
/// Temporary power-up that makes the player invisible to enemies
/// for a specified number of turns.
/// 
/// While active, the player's invisible trait is applied and
/// removed once the duration expires.
/// 
/// Responsibilities:
/// - Apply invisibility trait to the player
/// - Track remaining turns
/// - Play activation and completion animations
/// </summary>

namespace DontWitchMeNow
{
    public class InvisibilityCloak : PowerBase
    {
        [LESerializeField] public int LEcloakMoves = 5;

        public override void Init(Player p, PopupFields popup = null)
        {
            base.Init(p, popup);

            /* 
             * turnsDuration derived from PowerBase
             * LEcloakMoves connected to a in-house made level editor tool 
             * */
            turnsDuration = LEcloakMoves; 
        }

        public override void ActivateEffect()
        {
            transform.position = player.transform.position;
            powerAnimator.SetTrigger("Activate"); // powerAnimator derived from PowerBase
            player.traits.AddTraits(TraitsList.INVISIBLE);
            AddPowerEvent();
        }

        protected override void UpdatePowerState()
        {
            turnsDuration--;

            if (turnsDuration == 0)
                PowerComplete();
        }

        public override void PowerComplete(bool endImmediate = false)
        {
            transform.position = player.transform.position;
            powerAnimator.SetTrigger("End");
            player.traits.RemoveTrait(TraitsList.INVISIBLE);
            AddPowerEvent();
        }
    }
}