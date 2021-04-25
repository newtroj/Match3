using UnityEngine;

namespace GameBoard
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config", order = 0)]
    public class GameConfigScriptableObject : ScriptableObject
    {
        [SerializeField] private int _blockSize;
        [SerializeField] private int _boardWidth;
        [SerializeField] private int _boardHeight;

        public int BlockSize =>     _blockSize;
        public int BoardWidth =>    _boardWidth;
        public int BoardHeight =>   _boardHeight;
    }
}