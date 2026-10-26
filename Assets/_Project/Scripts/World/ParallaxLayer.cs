using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform[] _tiles;
        [SerializeField, Min(0.01f), Tooltip("Rendered tile width in world units, including Transform scale.")]
        private float _tileWidthWorldUnits = 20f;
        [SerializeField, Range(0.01f, 1f)] private float _speedFactor = 0.2f;

        private Vector3[] _startPositions;
        private double _offset;
        public float SpeedFactor => _speedFactor;

        public bool Initialize(Camera camera)
        {
            if (!SetupValidation.Check(_tiles != null && _tiles.Length >= 2,
                this, "Assign at least two repeated tiles.")) return false;
            float cameraWidth = camera.orthographicSize * 2f * camera.aspect;
            bool valid = SetupValidation.Check((_tiles.Length - 1) * _tileWidthWorldUnits >= cameraWidth,
                this, "Add tiles: all tiles except one must cover the full camera width.");
            _startPositions = new Vector3[_tiles.Length];
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (!SetupValidation.Require(_tiles[i], this, $"Tiles[{i}]")) return false;
                _startPositions[i] = _tiles[i].position;
                valid &= SetupValidation.Check(i == 0 || Mathf.Abs(_tiles[i].position.x
                    - _tiles[i - 1].position.x - _tileWidthWorldUnits) < 0.01f,
                    this, "List tiles left to right, exactly one Tile Width apart.");
            }

            valid &= SetupValidation.Check(_tiles[0].position.x <= camera.transform.position.x - cameraWidth / 2f,
                this, "The first tile pivot must be at or left of the camera's left edge.");
            valid &= SetupValidation.Check(_tiles[0].position.x + (_tiles.Length - 1) * _tileWidthWorldUnits
                >= camera.transform.position.x + cameraWidth / 2f,
                this, "The strip needs enough extra width to cover the right edge immediately before wrapping.");
            return valid;
        }

        public void ResetRun()
        {
            _offset = 0d;
            for (int i = 0; i < _tiles.Length; i++) _tiles[i].position = _startPositions[i];
        }

        public void Scroll(float worldDistance)
        {
            double stripWidth = _tileWidthWorldUnits * _tiles.Length;
            _offset = (_offset + worldDistance * _speedFactor) % stripWidth;
            for (int i = 0; i < _tiles.Length; i++)
            {
                // Wrap by the whole strip and keep the fractional overshoot.
                double relative = i * _tileWidthWorldUnits - _offset;
                if (relative <= -_tileWidthWorldUnits) relative += stripWidth;
                Vector3 position = _startPositions[i];
                position.x = _startPositions[0].x + (float)relative;
                _tiles[i].position = position;
            }
        }
    }
}
