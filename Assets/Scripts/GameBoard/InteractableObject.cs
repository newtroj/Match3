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
        
        //TODO There is a better way to do it, but for test purpose it's ok
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
            Bread = 4,
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
                //TODO just for debug purpose, remove it
                _image.color = Color.gray;
                ItIsOnAMatch = false;
            }
        }

        //TODO will reset object state when destroyed by match
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
            //TODO need a better way to find a kind, it's ok for tests
            //TODO need to check the board before start the game to avoid start with a match
            //don't need to convert to index because Random.Range has minInclusive and maxExclusive
            //min value is 1 because 0 is None on Kind Enum
            int interactableObjects = Enum.GetNames(typeof(Kind)).Length;
            ObjectKind = (Kind) Random.Range(1, interactableObjects);

            UpdateKind();
        }

        private void UpdateKind()
        {
            //TODO move UI to UI script
            _image.sprite = _config.ObjectList[(int) ObjectKind];
            
            //updating name here because it's better to debug the objects with the kind in the name 
            UpdateName();
        }

        private void SetPosition()
        {
            //TODO use anchors properly to support different resolutions 
            _rectTransform.anchoredPosition = new Vector2(HorizontalIndex * _config.InteractableObjectSize, VerticalIndex * _config.InteractableObjectSize);
        }

        private void OnInteractableDroppedOnMeReceived(PointerEventData eventData)
        {
            //TODO did this way for test purpose, check if it's ok later
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
            
            //TODO ADD OnSwapAccepted method to put an event inside probably, and put swap objects method inside
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