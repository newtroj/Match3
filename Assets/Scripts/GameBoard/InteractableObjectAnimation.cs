using System;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace GameBoard
{
    public class InteractableObjectAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetRectTransform;
        
        private CanvasGroup _targetCanvasGroup;
        private InteractableObject _myInteractableObject;

        public event Action EvtMatchAnimationFinished;

        private void Awake()
        {
            _targetCanvasGroup = _targetRectTransform.GetComponent<CanvasGroup>();
            
            _myInteractableObject = GetComponent<InteractableObject>();
            _myInteractableObject.EvtMatchFound += EvtOnMatchFound;
        }

        private void OnDestroy()
        {
            _myInteractableObject.EvtMatchFound -= EvtOnMatchFound;
        }

        private void EvtOnMatchFound()
        {
            Tweener tweener = TweenUtils.DoCanvasGroupColor(_targetCanvasGroup, 0, 1);
            tweener.onComplete += OnMatchFoundAnimationCompleted;
        }

        private void OnMatchFoundAnimationCompleted()
        {
            EvtMatchAnimationFinished?.Invoke();
            _targetCanvasGroup.alpha = 1;
        }
    }
}