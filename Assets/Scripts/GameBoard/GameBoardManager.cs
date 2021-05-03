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
        public static event Action EvInteractableObjectMatchFound; 
        
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
            _interactableObjects = new InteractableObject[_gameConfig.BoardHeight, _gameConfig.BoardWidth];
            DoActionOnAllInteractableObjects(InstantiateBoard);
            DoActionOnAllInteractableObjects(SetupBoardNeighbors);
        }

        private void InitializeGameBoard()
        {
            do
            {
                ShuffleBoard();
            } while (!HasAnyPossibleMatch());
        }

        private void InstantiateBoard(int verticalIndex, int horizontalIndex)
        {
            GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
            InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
            interactableObject.SetupConfig(_gameConfig, verticalIndex, horizontalIndex);
            
            _interactableObjects[verticalIndex, horizontalIndex] = interactableObject;
        }
        
        private void SetupBoardNeighbors(int verticalIndex, int horizontalIndex)
        {
            InteractableObject neighborUp =     GetInteractableObjectInstance(verticalIndex - 1, horizontalIndex);
            InteractableObject neighborDown =   GetInteractableObjectInstance(verticalIndex + 1, horizontalIndex);
            InteractableObject neighborLeft =   GetInteractableObjectInstance(verticalIndex, horizontalIndex - 1);
            InteractableObject neighborRight =  GetInteractableObjectInstance(verticalIndex, horizontalIndex + 1);

            _interactableObjects[verticalIndex, horizontalIndex].SetupNeighbors(neighborUp, neighborDown, neighborLeft, neighborRight);
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
            ProcessingMatch();
        }

        private void ProcessingMatch()
        {
            if (!IsThereAMatch())
                return;

            DoObjectsMatchResult();

            while (!HasAnyPossibleMatch())
                ShuffleBoard();
        }

        private bool IsThereAMatch()
        {
            bool matchFound = false;
            
            //Searching for a match on lines
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    matchFound |= UpdateMatchHunter(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }

                matchFound |= CheckMatchFoundAndClear();
            }

            //searching for a match on columns
            for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
            {
                InteractableObject.Kind lastKind = InteractableObject.Kind.None;
                
                for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
                {
                    InteractableObject interactable = _interactableObjects[verticalIndex, horizontalIndex];
                    matchFound |= UpdateMatchHunter(interactable, lastKind);
                    lastKind = interactable.ObjectKind;
                }
                
                matchFound |= CheckMatchFoundAndClear();
            }

            return matchFound;
        }

        private bool CheckMatchFoundAndClear()
        {
            if (MatchFound)
            {
                FlagMatchFound();
                return true;
            }

            _matchHunterAux.Clear();
            return false;
        }

        private bool UpdateMatchHunter(InteractableObject interactable, InteractableObject.Kind lastKind)
        {
            InteractableObject.Kind currentKind = interactable.ObjectKind;

            if (currentKind != InteractableObject.Kind.None && (currentKind == lastKind || lastKind == InteractableObject.Kind.None))
            {
                //the hunt goes on
                _matchHunterAux.Push(interactable);
                return false;
            }

            //we have come to an end
            CheckMatchFoundAndClear();
            
            //Start a new hunt
            _matchHunterAux.Push(interactable);

            return true;
        }

        private void FlagMatchFound()
        {
            int stackCount = _matchHunterAux.Count;
            
            Debug.LogError($"MatchFound for {stackCount} Objects");
            for (int i = 0; i < stackCount; i++)
            {
                InteractableObject interactableObject = _matchHunterAux.Pop();
                Debug.Log($"FlagMatchFound:{interactableObject.name}");
                interactableObject.FlagMatchFound();
            }
        }
        
        private void DoObjectsMatchResult()
        {
            DoActionOnAllInteractableObjects(OnInteractableObjectsMatchFound);
        }

        private void OnInteractableObjectsMatchFound(int verticalIndex, int horizontalIndex)
        {
            InteractableObject interactableObject = _interactableObjects[verticalIndex, horizontalIndex];
            if(!interactableObject._matchFound)
                return;
            
            interactableObject.OnMatchFound();
            
            EvInteractableObjectMatchFound?.Invoke();
        }

        private void DoActionOnAllInteractableObjects(Action<int, int> action)
        {
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    action(verticalIndex, horizontalIndex);
                }
            }
        }
    }
}