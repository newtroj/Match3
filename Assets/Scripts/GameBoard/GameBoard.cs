using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameBoard
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private Transform _interactableObjectsParent;
        [SerializeField] private GameObject _interactableObjectPrefab;

        [SerializeField] private GameConfigScriptableObject _gameConfig;
        
        private InteractableObject[,] _blocks;
        
        void Start()
        {
            SetupBoard();
        }

        private void SetupBoard()
        {
            _blocks = new InteractableObject[_gameConfig.BoardWidth, _gameConfig.BoardHeight];
            
            int interactableObjectsMaxIndex = Enum.GetNames(typeof(InteractableObject.Kind)).Length - 1;
            
            for (int i = 0; i < _gameConfig.BoardWidth; i++)
            {
                for (int j = 0; j < _gameConfig.BoardHeight; j++)
                {
                    GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
                    
                    int objectKind = GetRandomObjectKind(interactableObjectsMaxIndex);
                    
                    InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
                    interactableObject.Setup(_gameConfig, i, j, objectKind);

                    _blocks[i, j] = interactableObject;
                }
            }
        }
        private int GetRandomObjectKind(int interactableObjectsMaxIndex)
        {
            return Random.Range(0, interactableObjectsMaxIndex);
        }
    }
}