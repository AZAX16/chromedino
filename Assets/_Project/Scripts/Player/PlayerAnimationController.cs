using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent, RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsHit = Animator.StringToHash("IsHit");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int IsDashing = Animator.StringToHash("IsDashing");
        private static readonly int RunState = Animator.StringToHash("Base Layer.Run");

        private Animator _animator;
        private PlayerController _player;

        public bool Initialize(PlayerController player)
        {
            _animator = GetComponent<Animator>();
            _player = player;
            if (!SetupValidation.Require(_animator.runtimeAnimatorController, this, "Animator Controller")) return false;
            bool valid = HasParameter(IsGrounded, AnimatorControllerParameterType.Bool)
                & HasParameter(VerticalVelocity, AnimatorControllerParameterType.Float)
                & HasParameter(IsHit, AnimatorControllerParameterType.Bool)
                & HasParameter(IsDead, AnimatorControllerParameterType.Bool)
                & HasParameter(IsDashing, AnimatorControllerParameterType.Bool);
            foreach (string state in new[] { "Run", "Jump", "Hit", "Death" })
            {
                valid &= SetupValidation.Check(_animator.HasState(0, Animator.StringToHash("Base Layer." + state)),
                    this, $"Animator needs Base Layer/{state}.");
            }

            _animator.applyRootMotion = false;
            _animator.updateMode = AnimatorUpdateMode.Normal;
            return valid;
        }

        public void Refresh()
        {
            bool dead = _player.Health.IsDead;
            _animator.SetBool(IsGrounded, _player.IsGrounded);
            _animator.SetFloat(VerticalVelocity, _player.VerticalVelocity);
            _animator.SetBool(IsDead, dead);
            _animator.SetBool(IsHit, !dead && _player.Health.IsRecovering);
            _animator.SetBool(IsDashing, !dead && _player.IsDashing);
        }

        public void ResetRun()
        {
            _animator.enabled = true;
            _animator.Rebind();
            Refresh();
            _animator.Play(RunState, 0, 0f);
            _animator.Update(0f);
        }

        public void SetVisibleAnimationEnabled(bool enabled) => _animator.enabled = enabled;

        private bool HasParameter(int hash, AnimatorControllerParameterType type)
        {
            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.nameHash == hash && parameter.type == type) return true;
            }

            return SetupValidation.Check(false, this, $"Missing Animator parameter hash {hash} ({type}); see Docs/UnitySetup.md.");
        }
    }
}
