using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameBoard
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private Transform _interactableObjectsParent;
        [SerializeField] private GameObject _interactableObjectPrefab;

        //TODO should I have another configuration scriptable for context purpose?
        [SerializeField] private GameConfigScriptableObject _gameConfig;
        
        private InteractableObject[,] _interactableObjects;
        
        //To avoid memory alloc
        private readonly Stack<InteractableObject> _matchHunterAux = new Stack<InteractableObject>(8);

        private bool MatchFound => _matchHunterAux.Count >= _gameConfig.MinimumObjectsForAMatch;
        
        void Start()
        {
            SetupBoard();
            CheckForAMatch();

            DraggableObject.EvtOnAnyInteractableDropped += OnAnyInteractableDropped;
        }

        private void OnDestroy()
        {
            DraggableObject.EvtOnAnyInteractableDropped -= OnAnyInteractableDropped;
        }

        private void SetupBoard()
        {
            _interactableObjects = new InteractableObject[_gameConfig.BoardWidth, _gameConfig.BoardHeight];
            
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
                    
                    InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
                    interactableObject.Setup(_gameConfig, verticalIndex, horizontalIndex);

                    _interactableObjects[verticalIndex, horizontalIndex] = interactableObject;
                }
            }
        }
        
        private void OnAnyInteractableDropped(PointerEventData eventData)
        {
            CheckForAMatch();
        }
        
        //TODO duplicated code! do it better! and don't sleep on the keyboard!
        private void CheckForAMatch()
        {
            //Searching for a match on lines
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    InteractableCheck(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }

                if(MatchFound)
                    FlagInteractableObjectsAsAMatch();
            }
            
            _matchHunterAux.Clear();
            
            //searching for a match on columns
            for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
            {
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    InteractableCheck(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }

                if(MatchFound)
                    FlagInteractableObjectsAsAMatch();
            }
            
            _matchHunterAux.Clear();
        }
        
        //TODO a better name pls
        private void InteractableCheck(InteractableObject interactable, InteractableObject.Kind lastKind)
        {
            InteractableObject.Kind currentKind = interactable.ObjectKind;
 
            if (currentKind != lastKind && lastKind != InteractableObject.Kind.None)
            {
                if (MatchFound)
                    FlagInteractableObjectsAsAMatch();
                else
                    _matchHunterAux.Clear();
                
                return;
            }

            _matchHunterAux.Push(interactable);
        }

        private void FlagInteractableObjectsAsAMatch()
        {
            int stackCount = _matchHunterAux.Count;
            for (int i = 0; i < stackCount; i++)
            {
                InteractableObject interactableObject = _matchHunterAux.Pop();
                interactableObject.ItIsOnAMatch = true;
            }
            
            _matchHunterAux.Clear();
        }
    }
}