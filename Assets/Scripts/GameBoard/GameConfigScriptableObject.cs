using System.Collections.Generic;
using UnityEngine;

namespace GameBoard
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config", order = 0)]
    public class GameConfigScriptableObject : ScriptableObject
    {
        [SerializeField] private int _blockSize;
        [SerializeField] private int _boardWidth;
        [SerializeField] private int _boardHeight;
        
        [Header("Gems")]
        [SerializeField] private List<Sprite> _objectList;

        public int BlockSize =>     _blockSize;
        public int BoardWidth =>    _boardWidth;
        public int BoardHeight =>   _boardHeight;
        public List<Sprite> ObjectList =>   _objectList;
    }
}