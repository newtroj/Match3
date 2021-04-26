using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace GameBoard
{
    public class InteractableObject : MonoBehaviour
    {
        //TODO UI here?
        [SerializeField] private Image _image;
        
        private RectTransform _rectTransform;
        private DraggableObject _draggableObject;
        private GameConfigScriptableObject _config;
        
        private Kind _objectKind;
        
        //TODO Remove it and do better
        public bool ItIsOnAMatch = false;
        
        public int InstanceID { get; private set; }
        public int VerticalIndex { get; private set; }
        public int HorizontalIndex { get; private set; }
        public Kind ObjectKind
        {
            get => _objectKind;
            private set
            {
                _objectKind = value;
                EvtKindChanged?.Invoke(_objectKind);
            }
        }
        
        public event Action<Kind> EvtKindChanged;
        
        public enum Kind
        {
            None =  0,
            White = 1,
            Red =   2,
            Orange = 3,
        }

        private void Awake()
        {
            InstanceID = GetInstanceID();
            _rectTransform = GetComponent<RectTransform>();
            _draggableObject = GetComponentInChildren<DraggableObject>();
            
            _draggableObject.EvtOnInteractableDroppedOnMe += OnInteractableDroppedOnMeReceived;
        }

        private void OnDestroy()
        {
            _draggableObject.EvtOnInteractableDroppedOnMe -= OnInteractableDroppedOnMeReceived;
        }

        private void Update()
        {
            if (ItIsOnAMatch)
            {
                _image.color = Color.gray;
                ItIsOnAMatch = false;
            }
        }

        private void ResetState()
        {
            _image.color = Color.white;
        }

        public void Setup(GameConfigScriptableObject gameConfig, int verticalIndex, int horizontalIndex)
        {
            _config = gameConfig;
            
            VerticalIndex = verticalIndex;
            HorizontalIndex = horizontalIndex;
            
            SetPosition();
            SetupKind();
        }

        private void UpdateName()
        {
            name = $"[{VerticalIndex}|{HorizontalIndex}] - {_objectKind.ToString()}";
        }

        private void SetupKind()
        {
            int interactableObjects = Enum.GetNames(typeof(Kind)).Length;
            ObjectKind = (Kind) Random.Range(1, interactableObjects);

            UpdateKind();
        }

        private void UpdateKind()
        {
            _image.sprite = _config.ObjectList[(int) ObjectKind];
            UpdateName();
        }

        private void SetPosition()
        {
            _rectTransform.anchoredPosition = new Vector2(HorizontalIndex * _config.InteractableObjectSize, VerticalIndex * _config.InteractableObjectSize);
        }

        private void OnInteractableDroppedOnMeReceived(PointerEventData eventData)
        {
            InteractableObject draggedInteractableObject = eventData.pointerDrag.transform.parent.GetComponent<InteractableObject>();
            
            if(draggedInteractableObject.InstanceID == InstanceID)
                return;

            CheckIfShouldSwapPositions(draggedInteractableObject);
        }
        
        private void CheckIfShouldSwapPositions(InteractableObject interactable)
        {
            int verticalDistance = Mathf.Abs(interactable.VerticalIndex - VerticalIndex);
            int horizontalDistance = Mathf.Abs(interactable.HorizontalIndex - HorizontalIndex);

            if (verticalDistance > 1 || horizontalDistance > 1 || (verticalDistance == 1 && horizontalDistance == 1))
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
            Kind tempKindVar = interactableObject.ObjectKind;
            interactableObject.ObjectKind = ObjectKind;
            ObjectKind = tempKindVar;

            UpdateKind();
            interactableObject.UpdateKind();
        }
    }
}