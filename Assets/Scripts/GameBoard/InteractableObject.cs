using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace GameBoard
{
    public class InteractableObject : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private DraggableObject _draggableObject;
        public GameConfigScriptableObject Config  { get; private set; }

        private InteractableObjectAnimation _interactableObjectAnimation;
        
        //TODO fix it
        public bool _matchFound = false;
        
        private InteractableObject NeighborUp { get; set; }
        private InteractableObject NeighborDown { get; set; }
        private InteractableObject NeighborLeft { get; set; }
        private InteractableObject NeighborRight { get; set; }
        
        public int InstanceID { get; private set; }
        public int VerticalIndex { get; private set; }
        public int HorizontalIndex { get; private set; }
        
        private Kind _objectKind;
        public Kind ObjectKind
        {
            get => _objectKind;
            private set
            {
                _objectKind = value;
                _matchFound = false;
                
                //updating name here because it's better to debug the objects with the kind in the name
                UpdateName();
                
                EvtKindChanged?.Invoke(_objectKind);
            }
        }
        
        public event Action<Kind> EvtKindChanged;
        public event Action<InteractableObject> EvtSwapSuccess;
        public event Action<InteractableObject> EvtSwapFail;
        public event Action<InteractableObject> EvtNextKindFound;
        public event Action EvtMatchFound;
        public static event Action EvtLastInteractableKindFoundForThisColumn;
        
        public enum Kind
        {
            None =  0,
            White = 1,
            Red =   2,
            Orange = 3,
            Yellow = 4,
            Green = 5,
            Brown = 6,
            Purple = 7,
        }

        private void Awake()
        {
            InstanceID = GetInstanceID();
            _rectTransform = GetComponent<RectTransform>();
            
            _draggableObject = GetComponentInChildren<DraggableObject>();
            _draggableObject.EvtOnInteractableDroppedOnMe += OnInteractableDroppedReceived;

            _interactableObjectAnimation = GetComponent<InteractableObjectAnimation>();
            _interactableObjectAnimation.EvtMatchAnimationFinished += OnMatchAnimationFinished;
            
            GameBoardManager.EvtBoardShuffled += OnSetupInteractableObjectsShuffled;
        }

        private void OnDestroy()
        {
            _interactableObjectAnimation.EvtMatchAnimationFinished -= OnMatchAnimationFinished;
            _draggableObject.EvtOnInteractableDroppedOnMe -= OnInteractableDroppedReceived;
            GameBoardManager.EvtBoardShuffled -= OnSetupInteractableObjectsShuffled;
        }

        public void SetupConfig(GameConfigScriptableObject gameConfig, int verticalIndex, int horizontalIndex)
        {
            Config = gameConfig;
            
            VerticalIndex = verticalIndex;
            HorizontalIndex = horizontalIndex;
            
            SetPosition();
        }

        public void SetupNeighbors(InteractableObject up, InteractableObject down, InteractableObject left, InteractableObject right)
        {
            NeighborUp = up;
            NeighborDown = down;
            NeighborLeft = left;
            NeighborRight = right;
        }

        private void OnSetupInteractableObjectsShuffled()
        {
            while (HasAnyMatch()) 
                SetupNewKind();
        }

        private void UpdateName()
        {
            name = $"[{VerticalIndex}|{HorizontalIndex}] - {_objectKind.ToString()}";
        }

        public Kind GetNewKind()
        {
            //Added +1 because Random.Range has maxExclusive param
            int interactableObjects = Config.ObjectList.Count;
            return (Kind) Random.Range(1, interactableObjects);
        }
        
        public void SetupNewKind()
        {
            ObjectKind = GetNewKind();
        }

        private void SetPosition()
        {
            //TODO use anchors properly to support different resolutions 
            _rectTransform.anchoredPosition = new Vector2(
                Config.InteractableObjectSize*0.5f + (HorizontalIndex * Config.InteractableObjectSize),
                -Config.InteractableObjectSize*0.5f - (VerticalIndex * Config.InteractableObjectSize));
        }

        private void OnInteractableDroppedReceived(PointerEventData eventData)
        {
            InteractableObject draggedInteractableObject = eventData.pointerDrag.transform.parent.GetComponent<InteractableObject>();
            
            if(draggedInteractableObject.InstanceID == InstanceID)
                return;

            if (CanSwap(draggedInteractableObject))
            {
                OnSwapAccepted(draggedInteractableObject);
                return;
            }
            
            OnSwapDenied(draggedInteractableObject);
        }
        
        private bool CanSwap(InteractableObject interactable)
        {
            int verticalDistance = Mathf.Abs(interactable.VerticalIndex - VerticalIndex);
            int horizontalDistance = Mathf.Abs(interactable.HorizontalIndex - HorizontalIndex);

            //Swap for now, just to test if it will be a match
            DoSwap(interactable);
            bool hasAnyMatchAfterSwap = HasAnyMatch() || interactable.HasAnyMatch();
            
            //swap back
            DoSwap(interactable);
            
            return verticalDistance <= 1 && horizontalDistance <= 1 && verticalDistance != horizontalDistance && hasAnyMatchAfterSwap;
        }

        private void OnSwapDenied(InteractableObject interactableObject)
        {
            EvtSwapFail?.Invoke(interactableObject);
        }
        
        private void OnSwapAccepted(InteractableObject interactableObject)
        {
            DoSwap(interactableObject);
            EvtSwapSuccess?.Invoke(interactableObject);
        }

        private void DoSwap(InteractableObject interactableObject)
        {
            Kind tempKindVar = interactableObject.ObjectKind;
            interactableObject.ObjectKind = ObjectKind;
            ObjectKind = tempKindVar;
        }

        public bool HasAnyPossibleMatch()
        {
            return HasVerticalPossibleMatch() || HasHorizontalPossibleMatch();
        }

        public bool HasVerticalPossibleMatch()
        {
            bool possibleMatchUp = NeighborDown && NeighborDown.NeighborDown && ItIsAMatch(ObjectKind, NeighborUp, NeighborDown.NeighborDown);//xXox
            bool possibleMatchDown = NeighborUp && NeighborUp.NeighborUp && ItIsAMatch(ObjectKind, NeighborUp.NeighborUp, NeighborDown);//xoXx

            return possibleMatchUp || possibleMatchDown;
        }

        public bool HasHorizontalPossibleMatch()
        {
            bool possibleMatchLeft = NeighborLeft && NeighborLeft.NeighborLeft && ItIsAMatch(ObjectKind, NeighborRight, NeighborLeft);//xoXx
            bool possibleMatchRight = NeighborLeft && NeighborLeft.NeighborLeft && ItIsAMatch(ObjectKind, NeighborLeft, NeighborLeft);//xXox
            
            return possibleMatchLeft || possibleMatchRight;
        }
        
        public bool HasAnyMatch()
        {
            return HasVerticalMatch() || HasHorizontalMatch();
        }

        public bool HasVerticalMatch()
        {
            bool hasMatchMiddle = ItIsAMatch(ObjectKind, NeighborUp, NeighborDown);//xXx
            bool hasMatchUp = NeighborUp && ItIsAMatch(ObjectKind, NeighborUp, NeighborUp.NeighborUp);//xxX
            bool hasMatchDown = NeighborDown && ItIsAMatch(ObjectKind, NeighborDown, NeighborDown.NeighborDown);//Xxx

            return hasMatchUp || hasMatchMiddle || hasMatchDown;
        }

        public bool HasHorizontalMatch()
        {
            bool hasMatchMiddle = ItIsAMatch(ObjectKind, NeighborRight, NeighborLeft);//xXx
            bool hasMatchLeft = NeighborLeft && ItIsAMatch(ObjectKind, NeighborLeft, NeighborLeft.NeighborLeft);//xxX
            bool hasMatchRight = NeighborRight && ItIsAMatch(ObjectKind, NeighborRight, NeighborRight.NeighborRight);//Xxx
            
            return hasMatchLeft || hasMatchMiddle || hasMatchRight;
        }
        
        private bool ItIsAMatch(Kind objKind1, InteractableObject obj1, InteractableObject obj2)
        {
            return obj1 && obj2 && objKind1 == obj1.ObjectKind && objKind1 == obj2.ObjectKind;
        }
        
        public void FlagMatchFound()
        {
            _matchFound = true;
        }
        
        public void OnMatchFound()
        {
            _matchFound = false;
            ObjectKind = Kind.None;
            EvtMatchFound?.Invoke();
        }
        
        private void OnMatchAnimationFinished()
        {
            GetNextValidInteractableKind();
        }
        
        private void GetNextValidInteractableKind()
        {
            InteractableObject target = NeighborUp;

            while (target)
            {
                if (target.ObjectKind != Kind.None) 
                    break;

                target = target.NeighborUp;
            }

            OnNextInteractableKindFound(target);
            
            if(target)
                target.GetNextValidInteractableKind();
            else
            {
                //When target is null we have reached the last InteractableObject of this column
                EvtLastInteractableKindFoundForThisColumn?.Invoke();
            }
        }

        private void OnNextInteractableKindFound(InteractableObject target)
        {
            if (target)
                DoSwap(target);
            else
                SetupNewKind();

            EvtNextKindFound?.Invoke(target);
        }
    }
}