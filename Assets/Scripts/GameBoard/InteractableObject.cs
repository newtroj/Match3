using UnityEngine;

namespace GameBoard
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        
        private GameConfigScriptableObject _config;

        private int _verticalIndex;
        private int _horizontalIndex;

        public void Setup(GameConfigScriptableObject gameConfig, int verticalIndex, int horizontalIndex)
        {
            _config = gameConfig;
            
            _verticalIndex = verticalIndex;
            _horizontalIndex = horizontalIndex;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            _rectTransform.anchoredPosition = new Vector2(_horizontalIndex * _config.BlockSize, _verticalIndex * _config.BlockSize);
        }
    }
}