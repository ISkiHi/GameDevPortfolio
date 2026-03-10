/// <summary>
/// Black Hole Power
/// 
/// Throwable power-up that destroys entities within a specified
/// range when activated.
/// 
/// The visual scale of the effect adjusts dynamically based on
/// the configured effect radius.
/// </summary>

namespace DontWitchMeNow
{
    public class BlackHole : ThrowablePower
    {
        [LESerializeField] public new int effectRange = 1;
        [SerializeField] private float scaleOffset = 0.75f;

        public override void ActivateEffect()
        {
            Vector3 position = player.gridManager.GetTilePos(activationPos);
            position.z = transform.position.z;
            transform.position = position;
            transform.localScale = Vector3.one * Mathf.Clamp(effectRange * scaleOffset, 1, float.MaxValue);
        }
    }
}