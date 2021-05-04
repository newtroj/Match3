using GameBoard;
using Round;
using UnityEngine;
using UnityEngine.UI;

namespace GameFlow
{
    public class GameFlowUI : MonoBehaviour
    {
        [SerializeField] private Text _roundTimerText;
        [SerializeField] private Text _currentPointsText;
        [SerializeField] private Text _goalPointsText;
        [SerializeField] private Text _highScoreText;

        private int _lastRoundTimeLeftSecondsReceived = -1;
        
        private void Awake()
        {
            RoundManager.EvtRoundStarted += EvtOnRoundStarted;
            RoundManager.EvtRoundFinishFailed += EvtOnRoundFinished;
            RoundManager.EvtRoundFinishSuccess += EvtOnRoundFinished;
            
            GameBoardManager.EvInteractableObjectMatchFound += EvOnInteractableObjectMatchFound;
        }

        private void OnDestroy()
        {
            RoundManager.EvtRoundStarted -= EvtOnRoundStarted;
            RoundManager.EvtRoundFinishFailed -= EvtOnRoundFinished;
            RoundManager.EvtRoundFinishSuccess -= EvtOnRoundFinished;
            
            GameBoardManager.EvInteractableObjectMatchFound -= EvOnInteractableObjectMatchFound;
        }

        private void Update()
        {
            if(!RoundManager.Instance.IsRoundActive)
                return;

            int roundTimeLeftSeconds = RoundManager.Instance.GetRoundTimeLeft();
            if(roundTimeLeftSeconds == _lastRoundTimeLeftSecondsReceived)
                return;

            int minutes = roundTimeLeftSeconds / 60;
            int seconds = roundTimeLeftSeconds - (minutes * 60); // better than mod probably

            _roundTimerText.text = $"{minutes:00}:{seconds:00}";
            _lastRoundTimeLeftSecondsReceived = roundTimeLeftSeconds;
        }
        
        private void EvtOnRoundStarted()
        {
            SetCurrentScore();
            SetGoalPoints();
        }
        
        private void EvtOnRoundFinished()
        {
            SetHighScore();
        }

        private void EvOnInteractableObjectMatchFound()
        {
            SetCurrentScore();
        }

        private void SetGoalPoints()
        {
            _goalPointsText.text = $"{GameFlowManager.Instance.GetCurrentPointsGoal}";
        }
        
        private void SetHighScore()
        {
            _highScoreText.text = $"{RoundManager.Instance.HighScore}";
        }
        private void SetCurrentScore()
        {
            _currentPointsText.text = $"{RoundManager.Instance.MatchPoints}";
        }
    }
}