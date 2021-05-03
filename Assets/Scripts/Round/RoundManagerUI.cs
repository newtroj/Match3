using GameBoard;
using UnityEngine;
using UnityEngine.UI;

namespace Round
{
    public class RoundManagerUI : MonoBehaviour
    {
        [SerializeField] private Text _roundTimer;
        [SerializeField] private Text _pointsTimer;

        private RoundManager _roundManager;
        private int _lastRoundTimeLeftSecondsReiceved = -1;
        
        private void Awake()
        {
            _roundManager = GetComponent<RoundManager>();
            GameBoardManager.EvtObjectsMatchFound += EvtOnObjectsMatchFound;
        }

        private void OnDestroy()
        {
            GameBoardManager.EvtObjectsMatchFound -= EvtOnObjectsMatchFound;
        }
        
        private void Update()
        {
            if(!_roundManager.IsRoundActive)
                return;

            int roundTimeLeftSeconds = _roundManager.GetRoundTimeLeft();
            if(roundTimeLeftSeconds == _lastRoundTimeLeftSecondsReiceved)
                return;

            int minutes = roundTimeLeftSeconds / 60;
            int seconds = roundTimeLeftSeconds - (minutes * 60); // better than mod probably

            _roundTimer.text = $"{minutes:00}:{seconds:00}";
            _lastRoundTimeLeftSecondsReiceved = roundTimeLeftSeconds;
        }
        
        private void EvtOnObjectsMatchFound(int objectsCount)
        {
            _pointsTimer.text = $"{_roundManager.MatchPoints}";
        }
    }
}