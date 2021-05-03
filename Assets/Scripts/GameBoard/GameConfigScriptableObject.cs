﻿using System.Collections.Generic;
using UnityEngine;

namespace GameBoard
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config", order = 0)]
    public class GameConfigScriptableObject : ScriptableObject
    {
        [SerializeField] private int _boardWidth;
        [SerializeField] private int _boardHeight;
        [SerializeField] private int _interactableObjectSize;
        [SerializeField] private int _minimumObjectsForAMatch;
        [SerializeField] private int _roundTime;
        [SerializeField] private int _pointsPerObject;
        
        [Header("Gems")]
        [SerializeField] private List<Sprite> _objectList;

        public int BoardWidth =>                _boardWidth;
        public int BoardHeight =>               _boardHeight;
        public int InteractableObjectSize =>    _interactableObjectSize;
        public int MinimumObjectsForAMatch =>   _minimumObjectsForAMatch;
        public int RoundTime =>   _roundTime;
        public int PointsPerObject =>   _pointsPerObject;
        public List<Sprite> ObjectList =>       _objectList;
    }
}