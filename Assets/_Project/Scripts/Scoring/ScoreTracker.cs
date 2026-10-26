using System;
using UnityEngine;

namespace ChromeDino
{
    [DisallowMultipleComponent]
    public sealed class ScoreTracker : MonoBehaviour
    {
        private IHighScoreStore _store;
        private double _distance;
        private long _itemPoints;
        private float _pointsPerWorldUnit;
        private bool _recordDirty;
        private bool _acceptScoring;

        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public double Distance => _distance;
        public event Action ScoreChanged;

        public void Initialize(float pointsPerWorldUnit, IHighScoreStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _pointsPerWorldUnit = Mathf.Max(0f, pointsPerWorldUnit);
            HighScore = Math.Max(0, store.Load());
            ResetRun();
        }

        public void SetScoringEnabled(bool enabled) => _acceptScoring = enabled;

        public void ResetRun()
        {
            FlushHighScore();
            _acceptScoring = false;
            _distance = 0d;
            _itemPoints = 0;
            CurrentScore = 0;
            ScoreChanged?.Invoke();
        }

        public void SetDistance(double totalDistance)
        {
            if (!_acceptScoring || double.IsNaN(totalDistance) || totalDistance < _distance)
            {
                return;
            }

            _distance = totalDistance;
            UpdateScore(false);
        }

        public void AddItemPoints(int points)
        {
            if (!_acceptScoring || points <= 0)
            {
                return;
            }

            _itemPoints = Math.Min(int.MaxValue, _itemPoints + points);
            UpdateScore(true);
        }

        public void FlushHighScore()
        {
            if (!_recordDirty || _store == null)
            {
                return;
            }

            _store.Save(HighScore);
            _recordDirty = false;
        }

        private void UpdateScore(bool collectedItem)
        {
            double distancePoints = _pointsPerWorldUnit == 0f ? 0d : Math.Floor(_distance * _pointsPerWorldUnit);
            int score = (int)Math.Min(int.MaxValue, distancePoints + _itemPoints);
            bool changed = score != CurrentScore;
            CurrentScore = score;
            if (score > HighScore)
            {
                HighScore = score;
                _recordDirty = true;
            }

            if (changed || collectedItem)
            {
                ScoreChanged?.Invoke();
            }
        }
    }
}
