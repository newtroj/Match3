using System;
using GameBoard;
using UnityEngine;

namespace Round
{
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private GameConfigScriptableObject _config;

        public float _matchStartedTime = -1;
        public float MatchPoints { get; private set; }

        public static event Action EvtRoundStarted;
        public static event Action EvtRoundFinished;

        public bool IsRoundActive => _matchStartedTime >= 0;

        private void Awake()
        {
            GameBoardManager.EvtObjectsMatchFound += EvtObjectsOnMatchFound;
        }

        private void OnDestroy()
        {
            GameBoardManager.EvtObjectsMatchFound -= EvtObjectsOnMatchFound;
        }

        private void Update()
        {
            if(!IsRoundActive)
                return;
            
            if (GetRoundElapsedTime() > _config.RoundTime) 
                FinishRound();
        }

        /// <summary>
        /// Invoked on Button click
        /// </summary>
        public void StartRound()
        {
            _matchStartedTime = Time.time;
            MatchPoints = 0;
            EvtRoundStarted?.Invoke();
        }

        private void FinishRound()
        {
            _matchStartedTime = -1;
            EvtRoundFinished?.Invoke();
        }

        public float GetRoundElapsedTime()
        {
            return Time.time - _matchStartedTime;
        }
        
        public int GetRoundTimeLeft()
        {
            return (int) (_config.RoundTime - GetRoundElapsedTime());
        }
        
        private void EvtObjectsOnMatchFound(int objectsCount)
        {
            MatchPoints += _config.PointsPerObject * objectsCount;
        }
    }
}