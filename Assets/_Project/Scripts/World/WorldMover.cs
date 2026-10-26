using System.Collections.Generic;
using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class WorldMover : MonoBehaviour
    {
        private readonly List<RaycastHit2D> _sweepHits = new List<RaycastHit2D>(4);
        private readonly List<Collider2D> _overlaps = new List<Collider2D>(4);
        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private Interactable _interactable;
        private PlayerController _player;
        private ContactFilter2D _playerFilter;

        public void Initialize(Interactable interactable, PlayerController player)
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _interactable = interactable;
            _player = player;
            _playerFilter = new ContactFilter2D();
            _playerFilter.SetLayerMask(1 << player.gameObject.layer);
            _playerFilter.useTriggers = true;
        }

        public void Step(float distance, float cleanupX)
        {
            if (!gameObject.activeSelf) return;
            // Continuous collision mode alone does not guarantee trigger callbacks.
            // The sweep and trigger callback enter the same idempotent dispatch path.
            int overlapCount = _collider.Overlap(_playerFilter, _overlaps);
            for (int i = 0; i < overlapCount; i++)
            {
                if (_overlaps[i].attachedRigidbody == _player.Body)
                {
                    _interactable.TryInteract(_player);
                    break;
                }
            }

            if (!gameObject.activeSelf || _player.Health.IsDead) return;
            int hitCount = _collider.Cast(Vector2.left, _playerFilter, _sweepHits, distance);
            for (int i = 0; i < hitCount; i++)
            {
                if (_sweepHits[i].rigidbody == _player.Body)
                {
                    _interactable.TryInteract(_player);
                    break;
                }
            }

            if (!gameObject.activeSelf || _player.Health.IsDead) return;
            _body.MovePosition(_body.position + Vector2.left * distance);
            if (_collider.bounds.max.x - distance < cleanupX) _interactable.Despawn();
        }

        public void StopMotion()
        {
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.simulated = false;
        }
    }
}
