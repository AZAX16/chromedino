namespace ChromeDino
{
    public abstract class Obstacle : Interactable
    {
        protected override void OnInteract(PlayerController player) => player.Health.TryTakeDamage();
    }
}
