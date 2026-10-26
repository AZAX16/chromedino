namespace ChromeDino
{
    public sealed class CoinCollectible : Interactable
    {
        protected override void OnInteract(PlayerController player)
        {
            Session.Score.AddItemPoints(Session.Settings.CoinPoints);
            Despawn();
        }
    }
}
