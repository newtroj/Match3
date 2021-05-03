using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace GameBoard
{
    public class InteractableObjectUI : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private CanvasGroup _imageCanvasGroup;
        
        private InteractableObject _myInteractableObject;
        
        private void Awake()
        {
            _imageCanvasGroup = _image.GetComponent<CanvasGroup>();
            
            _myInteractableObject = GetComponent<InteractableObject>();
            _myInteractableObject.EvtKindChanged += OnKindUpdated;
            _myInteractableObject.EvtSwapSuccess += OnSwapSuccess;
            _myInteractableObject.EvtSwapFail += OnSwapFail;
        }
        
        private void OnDestroy()
        {
            _myInteractableObject.EvtKindChanged -= OnKindUpdated;
            _myInteractableObject.EvtSwapSuccess -= OnSwapSuccess;
            _myInteractableObject.EvtSwapFail -= OnSwapFail;
        }
        
        private void OnKindUpdated(InteractableObject.Kind kind)
        {
            _image.sprite = _myInteractableObject.Config.ObjectList[(int) kind];
        }
        
        private void OnSwapSuccess(InteractableObject obj)
        {
            
        }

        private void OnSwapFail(InteractableObject obj)
        {
            
        }

        private void OnMatch()
        {
            
        }
    }
}