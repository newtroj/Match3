using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameBoard
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private Transform _interactableObjectsParent;
        [SerializeField] private GameObject _interactableObjectPrefab;

        [SerializeField] private GameConfigScriptableObject _gameConfig;
        
        private InteractableObject[,] _interactableObjects;
        
        private readonly Stack<InteractableObject> _matchHunterAux = new Stack<InteractableObject>(8);

        public enum BoardDirections
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
        }
        
        void Start()
        {
            SetupBoard();
            CheckForMatch();

            DraggableObject.EvtOnAnyInteractableDropped += OnAnyInteractableDropped;
        }

        private void OnAnyInteractableDropped(PointerEventData eventData)
        {
            CheckForMatch();
        }

        private void OnDestroy()
        {
            DraggableObject.EvtOnAnyInteractableDropped -= OnAnyInteractableDropped;
        }

        private void SetupBoard()
        {
            _interactableObjects = new InteractableObject[_gameConfig.BoardWidth, _gameConfig.BoardHeight];
            
            for (int i = 0; i < _gameConfig.BoardHeight; i++)
            {
                for (int j = 0; j < _gameConfig.BoardWidth; j++)
                {
                    GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
                    
                    InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
                    interactableObject.Setup(_gameConfig, i, j);

                    _interactableObjects[i, j] = interactableObject;
                }
            }
        }
        
        private void CheckForMatch()
        {
            for (int i = 0; i < _gameConfig.BoardWidth; i++)
            {
                bool matchFound = false;
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int j = 0; j < _gameConfig.BoardHeight; j++)
                {
                    InteractableObject interactable = _interactableObjects[i, j];
                    InteractableObject.Kind currentKind = interactable.ObjectKind;

                    if (currentKind != lastKind && lastKind != InteractableObject.Kind.None)
                    {
                        if(matchFound)
                            FlagInteractableObjectsAsAMatch();
                        
                        _matchHunterAux.Clear();
                        matchFound = false;
                        lastKind = currentKind;
                        continue;
                    }

                    _matchHunterAux.Push(interactable);
                    lastKind = currentKind;

                    if (_matchHunterAux.Count >= _gameConfig.MinimumObjectsForAMatch)
                        matchFound = true;
                }

                if(matchFound)
                    FlagInteractableObjectsAsAMatch();
            }
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