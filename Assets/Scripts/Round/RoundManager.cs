using System;
using GameBoard;
using UnityEngine;

namespace Round
{
    public class RoundManager : MonoBehaviour
    {
        private int _pointsGoal;
        private int _pointsPerObject;
        private int _roundTimeInSeconds;
        private bool _isLastRound;
        private float _matchStartedTime = -1;

        public static RoundManager Instance;
        
        public int HighScore { get; private set; }
        public int MatchPoints { get; private set; }

        public static event Action EvtRoundStarted;
        public static event Action EvtRoundFinishSuccess;
        public static event Action EvtRoundFinishFailed;

        public bool IsRoundActive => _matchStartedTime > 0;

        private void Awake()
        {
            Instance = this;
            
            GameBoardManager.EvInteractableObjectMatchFound += EvOnInteractableObjectMatchFound;
        }

        private void OnDestroy()
        {
            Instance = null;
            
            GameBoardManager.EvInteractableObjectMatchFound -= EvOnInteractableObjectMatchFound;
        }

        private void Update()
        {
            if(!IsRoundActive)
                return;
            
            //timeout
            if (GetRoundElapsedTime() > _roundTimeInSeconds) 
                FinishRoundFailed();

            //Last round will finish on timeout
            if (MatchPoints >= _pointsGoal && !_isLastRound) 
                FinishRoundSuccess();
        }

        public void SetupRound(int roundTime, int pointsPerObject, int pointsGoal, bool isLastRound)
        {
            _isLastRound = isLastRound;
            _pointsGoal = pointsGoal;
            _pointsPerObject = pointsPerObject;
            _roundTimeInSeconds = roundTime;
        }
        
        public void StartRound()
        {
            _matchStartedTime = Time.time;
            EvtRoundStarted?.Invoke();
        }

        private void FinishRoundSuccess()
        {
            FinishRound();
            EvtRoundFinishSuccess?.Invoke();
        }
        
        private void FinishRoundFailed()
        {
            FinishRound();
            MatchPoints = 0;
            EvtRoundFinishFailed?.Invoke();
        }
        
        private void FinishRound()
        {
            if (MatchPoints > HighScore)
                HighScore = MatchPoints;
            
            _matchStartedTime = -1;
        }

        public float GetRoundElapsedTime()
        {
            return Time.time - _matchStartedTime;
        }
        
        public int GetRoundTimeLeft()
        {
            return (int) (_roundTimeInSeconds - GetRoundElapsedTime());
        }
        
        private void EvOnInteractableObjectMatchFound()
        {
            MatchPoints += _pointsPerObject;
        }
    }
}