using GameBoard;
using Round;
using UnityEngine;

namespace GameFlow
{
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] private GameConfigScriptableObject _config;

        private int _currentRound;
        
        public static GameFlowManager Instance;
        
        public int GetCurrentPointsGoal => _config.PointGoals[_currentRound];
        
        private void Awake()
        {
            Instance = this;
            
            RoundManager.EvtRoundFinishSuccess += EvtOnRoundFinishSuccess;
            RoundManager.EvtRoundFinishFailed += EvtOnRoundFinishFailed;
        }

        private void OnDestroy()
        {
            Instance = null;
            
            RoundManager.EvtRoundFinishSuccess -= EvtOnRoundFinishSuccess;
            RoundManager.EvtRoundFinishFailed -= EvtOnRoundFinishFailed;
        }

        public void StartGame()
        {
            SetupRound();
        }

        private void SetupRound()
        {
            int pointGoal = _config.PointGoals[_currentRound];
            bool isLastRound = _currentRound >= _config.PointGoals.Count-1;
            
            RoundManager.Instance.SetupRound(_config.RoundTime, _config.PointsPerObject, pointGoal, isLastRound);
            RoundManager.Instance.StartRound();
        }

        private void EvtOnRoundFinishSuccess()
        {
            Debug.Log($"[EvtOnRoundFinishSuccess]");
            _currentRound++;

            if (_currentRound >= _config.PointGoals.Count)
            {
                FinishGame();
                return;
            }
            
            SetupRound();
        }

        private void FinishGame()
        {
            _currentRound = 0;
        }

        private void EvtOnRoundFinishFailed()
        {
            Debug.Log($"[EvtOnRoundFinishFailed]");
            FinishGame();
        }
    }
}