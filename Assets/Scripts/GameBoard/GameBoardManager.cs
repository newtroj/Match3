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

        public static event Action EvtBoardSetupFinished; 
        public static event Action EvtBoardInitialized; 
        public static event Action EvtBoardShuffled; 
        public static event Action EvInteractableObjectMatchFound; 
        
        void Start()
        {
            SetupBoard();
            
            RoundManager.EvtRoundStarted += InitializeGameBoard;
            DraggableObject.EvtOnAnyInteractableDropped += OnAnyInteractableDropped;
            InteractableObject.EvtLastInteractableKindFoundForThisColumn += EvtOnFinishedColumnKindsUpdate;
        }

        private void OnDestroy()
        {
            RoundManager.EvtRoundStarted -= InitializeGameBoard;
            DraggableObject.EvtOnAnyInteractableDropped -= OnAnyInteractableDropped;
            InteractableObject.EvtLastInteractableKindFoundForThisColumn -= EvtOnFinishedColumnKindsUpdate;
        }

        /// <summary>
        /// if function returns false, we shall not pass anymore. break loop and return result
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        private bool DoActionOnAllInteractableObjects(Func<int, int, bool> function)
        {
            for (int verticalIndex = 0; verticalIndex < _gameConfig.BoardHeight; verticalIndex++)
            {
                for (int horizontalIndex = 0; horizontalIndex < _gameConfig.BoardWidth; horizontalIndex++)
                {
                    if (!function(verticalIndex, horizontalIndex))
                        return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Sometimes i don't need to break the loop, i am doing it again without that "if" for optimization purpose
        /// </summary>
        /// <param name="action"></param>
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
        
        private void SetupBoard()
        {
            InstantiateGameBoard();
            DoActionOnAllInteractableObjects(SetupBoardNeighborForInteractableObject);
            
            EvtBoardSetupFinished?.Invoke();
        }

        private void InstantiateGameBoard()
        {
            _interactableObjects = new InteractableObject[_gameConfig.BoardHeight, _gameConfig.BoardWidth];
            DoActionOnAllInteractableObjects(InstantiateInteractableObjectForIndex);
        }

        private void InstantiateInteractableObjectForIndex(int verticalIndex, int horizontalIndex)
        {
            GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
            InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
            interactableObject.SetupConfig(_gameConfig, verticalIndex, horizontalIndex);
            
            _interactableObjects[verticalIndex, horizontalIndex] = interactableObject;
        }
        
        private void SetupBoardNeighborForInteractableObject(int verticalIndex, int horizontalIndex)
        {
            InteractableObject neighborUp =     GetInteractableObjectInstance(verticalIndex - 1, horizontalIndex);
            InteractableObject neighborDown =   GetInteractableObjectInstance(verticalIndex + 1, horizontalIndex);
            InteractableObject neighborLeft =   GetInteractableObjectInstance(verticalIndex, horizontalIndex - 1);
            InteractableObject neighborRight =  GetInteractableObjectInstance(verticalIndex, horizontalIndex + 1);

            _interactableObjects[verticalIndex, horizontalIndex].SetupNeighbors(neighborUp, neighborDown, neighborLeft, neighborRight);
        }
        
        private void InitializeGameBoard()
        {
            do
            {
                ShuffleBoard();
            } 
            while (!HasAnyPossibleMatch());
            
            EvtBoardInitialized?.Invoke();
        }
        
        private void ShuffleBoard()
        {
            DoActionOnAllInteractableObjects(SetNewKindForInteractableObject);
            EvtBoardShuffled?.Invoke();
        }

        private void SetNewKindForInteractableObject(int verticalIndex, int horizontalIndex)
        {
            _interactableObjects[verticalIndex, horizontalIndex].SetupNewKind();
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
            FindAndProcessActiveMatches();
        }

        private void FindAndProcessActiveMatches()
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

        private void EvtOnFinishedColumnKindsUpdate()
        {
            bool hasFinishedAllKindUpdates = DoActionOnAllInteractableObjects(HasFinishedAllColumnKindUpdates);
            if (!hasFinishedAllKindUpdates) 
                return;
            
            FindAndProcessActiveMatches();
        }

        private bool HasFinishedAllColumnKindUpdates(int verticalIndex, int horizontalIndex)
        {
            return _interactableObjects[verticalIndex, horizontalIndex].ObjectKind != InteractableObject.Kind.None;
        }
    }
}