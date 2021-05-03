using System;
using System.Collections.Generic;
using Round;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameBoard
{
    public class GameBoardManager : MonoBehaviour
    {
        [SerializeField] private Transform _interactableObjectsParent;
        [SerializeField] private GameObject _interactableObjectPrefab;
        [SerializeField] private GameConfigScriptableObject _gameConfig;
        
        private InteractableObject[,] _interactableObjects;
        
        //To avoid memory alloc
        private readonly Stack<InteractableObject> _matchHunterAux = new Stack<InteractableObject>(8);

        private bool MatchFound => _matchHunterAux.Count >= _gameConfig.MinimumObjectsForAMatch;

        public static event Action EvtShuffleBoardFinished; 
        public static event Action<int> EvtObjectsMatchFound; 
        
        void Start()
        {
            SetupBoard();
            
            RoundManager.EvtRoundStarted += InitializeGameBoard;
            DraggableObject.EvtOnAnyInteractableDropped += OnAnyInteractableDropped;
        }

        private void OnDestroy()
        {
            RoundManager.EvtRoundStarted -= InitializeGameBoard;
            DraggableObject.EvtOnAnyInteractableDropped -= OnAnyInteractableDropped;
        }

        private void SetupBoard()
        {
            InstantiateBoard();
            SetupBoardNeighbors();
        }

        private void InitializeGameBoard()
        {
            do
            {
                ShuffleBoard();
            } while (!HasAnyPossibleMatch());
        }

        private void InstantiateBoard()
        {
            _interactableObjects = new InteractableObject[_gameConfig.BoardHeight, _gameConfig.BoardWidth];

            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
                    InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
                    interactableObject.SetupConfig(_gameConfig, verticalIndex, horizontalIndex);
                    
                    _interactableObjects[verticalIndex, horizontalIndex] = interactableObject;
                }
            }
        }
        
        private void SetupBoardNeighbors()
        {
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    InteractableObject neighborUp =     GetInteractableObjectInstance(verticalIndex + 1, horizontalIndex);
                    InteractableObject neighborDown =   GetInteractableObjectInstance(verticalIndex - 1, horizontalIndex);
                    InteractableObject neighborLeft =   GetInteractableObjectInstance(verticalIndex, horizontalIndex - 1);
                    InteractableObject neighborRight =  GetInteractableObjectInstance(verticalIndex, horizontalIndex + 1);

                    _interactableObjects[verticalIndex, horizontalIndex].SetupNeighbors(neighborUp, neighborDown, neighborLeft, neighborRight);
                }
            }
        }
        
        private void ShuffleBoard()
        {
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    _interactableObjects[verticalIndex, horizontalIndex].SetupStartingKind();
                }
            }
            
            EvtShuffleBoardFinished?.Invoke();
        }
        
        private bool HasAnyPossibleMatch()
        {
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    
                    if (!interactable.HasAnyPossibleMatch()) 
                        continue;
                    
                    return true;
                }
            }

            Debug.Log($"[CheckHasAnyPossibleMatch] Could not find any possible match");
            
            return false;
        }

        private InteractableObject GetInteractableObjectInstance(int verticalIndex, int horizontalIndex)
        {
            if (verticalIndex < 0 || verticalIndex >= _interactableObjects.GetLength(0) ||
                horizontalIndex < 0 || horizontalIndex >= _interactableObjects.GetLength(1))
                return null;
            
            return _interactableObjects[verticalIndex, horizontalIndex];
        }

        private void OnAnyInteractableDropped(PointerEventData eventData)
        {
            CheckForAMatch();

            while (!HasAnyPossibleMatch()) 
                ShuffleBoard();
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
                    UpdateMatchHunter(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }

                CheckMatchFoundAndClear();
            }

            //searching for a match on columns
            for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
            {
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    UpdateMatchHunter(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }
                
                CheckMatchFoundAndClear();
            }
        }

        private void CheckMatchFoundAndClear()
        {
            if (MatchFound)
                FlagInteractableObjectsAsAMatch();
            else
                _matchHunterAux.Clear();
        }

        private void UpdateMatchHunter(InteractableObject interactable, InteractableObject.Kind lastKind)
        {
            InteractableObject.Kind currentKind = interactable.ObjectKind;

            if (currentKind == lastKind || lastKind == InteractableObject.Kind.None)
            {
                //the hunt goes on
                _matchHunterAux.Push(interactable);
                return;
            }

            //we have come to an end
            CheckMatchFoundAndClear();
            
            //Start a new hunt
            _matchHunterAux.Push(interactable);
        }

        private void FlagInteractableObjectsAsAMatch()
        {
            int stackCount = _matchHunterAux.Count;
            for (int i = 0; i < stackCount; i++)
            {
                InteractableObject interactableObject = _matchHunterAux.Pop();
                // interactableObject.ItIsOnAMatch = true;
            }
            
            EvtObjectsMatchFound?.Invoke(stackCount);
        }
    }
}