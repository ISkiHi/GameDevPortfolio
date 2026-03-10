/// <summary>
/// Time Stop Shoes Power
/// 
/// Power-up that temporarily stops other entities in the level
/// while allowing the player to continue moving.
/// 
/// The effect lasts for a limited number of turns and triggers
/// activation and completion animations.
/// </summary>

namespace DontWitchMeNow
{
    public class TimeStopShoes : PowerBase
    {
        public override void ActivateEffect()
        {
            transform.position = new Vector2 (Camera.main.transform.position.x, Camera.main.transform.position.y);
            powerAnimator.SetTrigger("Activate"); // powerAnimated derived from PowerBase
            AddPowerEvent();
        }

        protected override void UpdatePowerState()
        {
            turnsDuration--;

            if (turnsDuration == 0) // turnsDuration derived from PowerBase
                PowerComplete();
        } 

        public override void PowerComplete(bool endImmediate = false)
        {
            transform.position = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
            powerAnimator.SetTrigger("End");
            AddPowerEvent();
        }
    }
}