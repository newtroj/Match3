using System;
using DG.Tweening;
using Round;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace GameBoard
{
    public class InteractableObjectUIAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetRectTransform;
        [SerializeField] private Image _image;
        
        private Tweener _currentHideTween;
        private CanvasGroup _targetCanvasGroup;
        private InteractableObject _myInteractableObject;
        
        public event Action EvtMatchAnimationFinished;

        private void Awake()
        {
            _targetCanvasGroup = _targetRectTransform.GetComponent<CanvasGroup>();
            
            RoundManager.EvtRoundStarted += EvtOnRoundStarted;
            RoundManager.EvtRoundFinishFailed += EvtOnRoundFinished;
            
            _myInteractableObject = GetComponent<InteractableObject>();
            _myInteractableObject.EvtMatchFound += EvtOnMatchFound;
            _myInteractableObject.EvtKindChanged += OnKindUpdated;
        }

        private void OnDestroy()
        {
            RoundManager.EvtRoundStarted -= EvtOnRoundStarted;
            RoundManager.EvtRoundFinishFailed -= EvtOnRoundFinished;
            
            _myInteractableObject.EvtMatchFound -= EvtOnMatchFound;
            _myInteractableObject.EvtKindChanged -= OnKindUpdated;
        }

        private void EvtOnRoundStarted()
        {
            Show();
        }
        
        private void EvtOnRoundFinished()
        {
            Hide();
        }

        private void EvtOnMatchFound()
        {
            PlayHideAnimation(OnMatchFoundAnimationCompleted);
        }

        private void PlayHideAnimation(Action callback)
        {
            _currentHideTween = TweenUtils.DoCanvasGroupColor(_targetCanvasGroup, 0, 1);
            _currentHideTween.onComplete += () => callback();
        }

        private void OnMatchFoundAnimationCompleted()
        {
            EvtMatchAnimationFinished?.Invoke();
            _targetCanvasGroup.alpha = 1;
        }

        private void OnKindUpdated(InteractableObject.Kind kind)
        {
            _image.sprite = _myInteractableObject.Config.ObjectList[(int) kind];
        }
        
        public void Hide()
        {
            _targetCanvasGroup.alpha = 0;
        }
        
        public void Show()
        {
            _targetCanvasGroup.alpha = 1;
        }
    }
}