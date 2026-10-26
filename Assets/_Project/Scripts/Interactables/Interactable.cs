using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D), typeof(WorldMover))]
    public abstract class Interactable : MonoBehaviour
    {
        private GameSession _session;
        private BoxCollider2D _interactionCollider;
        private bool _encountered;

        protected GameSession Session => _session;
        public WorldMover Mover { get; private set; }

        public void Initialize(GameSession session)
        {
            _session = session;
            _interactionCollider = GetComponent<BoxCollider2D>();
            Mover = GetComponent<WorldMover>();
            _encountered = false;
            _interactionCollider.enabled = true;
            Mover.Initialize(this, session.Player);
        }

        public bool TryInteract(PlayerController player)
        {
            if (_session == null || _session.State != GameState.Playing
                || player == null || player != _session.Player || player.Health.IsDead
                || !isActiveAndEnabled || _encountered || !_interactionCollider.enabled)
            {
                return false;
            }

            // Consuming even an invulnerable encounter prevents delayed repeat damage.
            _encountered = true;
            OnInteract(player);
            return true;
        }

        public void Despawn()
        {
            _interactionCollider.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        protected abstract void OnInteract(PlayerController player);
    }
}
