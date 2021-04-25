using UnityEngine;

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
            
            for (int i = 0; i < _gameConfig.BoardWidth; i++)
            {
                for (int j = 0; j < _gameConfig.BoardHeight; j++)
                {
                    GameObject itemInstance = Instantiate(_interactableObjectPrefab, _interactableObjectsParent);
                    
                    InteractableObject interactableObject = itemInstance.GetComponent<InteractableObject>();
                    interactableObject.Setup(_gameConfig, i, j);

                    _blocks[i, j] = interactableObject;
                }
            }
        }
    }
}