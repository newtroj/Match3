using System;
using UnityEngine;

namespace GameBoard
{
    public class InteractableObjectAnimation : MonoBehaviour
    {
        [SerializeField] private Animation _onMatchFoundAnimation;
        
        private InteractableObject _myInteractableObject;
        
        private void Awake()
        {
            _myInteractableObject.EvtMatchFound += EvtOnMatchFound;
        }

        private void OnDestroy()
        {
            _myInteractableObject.EvtMatchFound -= EvtOnMatchFound;
        }

        private void EvtOnMatchFound(Action callback)
        {
        }
    }
}