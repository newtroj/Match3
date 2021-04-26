using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

namespace GameBoard
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private Image _image;
        
        private RectTransform _rectTransform;
        private DraggableObject _draggableObject;
        private GameConfigScriptableObject _config;

        private int _verticalIndex;
        private int _horizontalIndex;
        private Kind _objectKind;

        private int _instanceID = -1;
        
        public enum Kind
        {
            None =  0,
            White = 1,
            Red =   2,
            Orange = 3,
        }

        private void Awake()
        {
            _instanceID = GetInstanceID();
            _rectTransform = GetComponent<RectTransform>();
            _draggableObject = GetComponent<DraggableObject>();
            
            _draggableObject.EvtOnDrop += OnDropReceived;
            _draggableObject.EvtEndDrag += OnEndDrag;
        }

        private void OnDestroy()
        {
            _draggableObject.EvtOnDrop -= OnDropReceived;
            _draggableObject.EvtEndDrag -= OnEndDrag;
        }

        public void Setup(GameConfigScriptableObject gameConfig, int verticalIndex, int horizontalIndex, int objectKind)
        {
            _config = gameConfig;
            
            _verticalIndex = verticalIndex;
            _horizontalIndex = horizontalIndex;
            _objectKind = (Kind) objectKind;

            UpdateName();
            SetupKind();
            SetPosition();
        }

        private void UpdateName()
        {
            name = $"[{_verticalIndex},{_horizontalIndex}]";
        }

        private void SetupKind()
        {
            _image.sprite = _config.ObjectList[(int)_objectKind];
        }
        
        private void SetPosition()
        {
            _rectTransform.anchoredPosition = new Vector2(_horizontalIndex * _config.BlockSize, _verticalIndex * _config.BlockSize);
            UpdateName();
        }
        
        private void OnEndDrag(PointerEventData eventData)
        {
            SetPosition();
        }

        private void OnDropReceived(PointerEventData eventData)
        {
            InteractableObject draggedInteractableObject = eventData.pointerDrag.GetComponent<InteractableObject>();
            
            if(draggedInteractableObject._instanceID == _instanceID)
                return;

            CheckIfShouldSwapPositions(draggedInteractableObject);
        }
        
        private void CheckIfShouldSwapPositions(InteractableObject interactable)
        {
            int verticalDistance = Mathf.Abs(interactable._verticalIndex - _verticalIndex);
            int horizontalDistance = Mathf.Abs(interactable._horizontalIndex - _horizontalIndex);

            if (verticalDistance > 1 || horizontalDistance > 1)
            {
                OnSwapDenied(interactable);
                return;
            }

            SwapObjectsPosition(interactable);
        }

        private void OnSwapDenied(InteractableObject interactableObject)
        {
            //TODO feedback event
        }
        
        private void SwapObjectsPosition(InteractableObject interactableObject)
        {
            int tempVar = -1;
            
            tempVar = interactableObject._verticalIndex;
            interactableObject._verticalIndex = _verticalIndex;
            _verticalIndex = tempVar;
            
            tempVar = interactableObject._horizontalIndex;
            interactableObject._horizontalIndex = _horizontalIndex;
            _horizontalIndex = tempVar;
            
            SetPosition();
            interactableObject.SetPosition();
        }
    }
}