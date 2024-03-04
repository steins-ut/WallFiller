using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    [Serializable]
    public class NextWalls
    {
        public int WallCount;
        public int HoleCount;

        public float Difficulty;
        public float DifficultyTolerance;

        [Tooltip("In seconds.")]
        public float SpawnTime;

        [Tooltip("In seconds.")]
        public float ArrivalTime;
    }

    [SerializeField]
    private int _padRows;

    [SerializeField]
    private int _padColumns;

    [SerializeField]
    private bool _hasLeftArm = false;

    [SerializeField]
    private float _leftArmRange;

    [SerializeField]
    private bool _hasRightArm = false;

    [SerializeField]
    private float _rightArmRange;

    [SerializeField]
    private bool _hasTopArm = false;

    [SerializeField]
    private float _topArmRange;

    [SerializeField]
    private bool _hasBottomArm = false;

    [SerializeField]
    private float _bottomArmRange;

    [SerializeField]
    private List<NextWalls> _nextWallsQueue = new List<NextWalls>();

    public int PadRows => _padRows;

    public int PadColumns => _padColumns;

    public bool HasLeftArm => _hasLeftArm;
    public float LeftArmRange => _leftArmRange;

    public bool HasRightArm => _hasRightArm;
    public float RightArmRange => _rightArmRange;

    public bool HasTopArm => _hasTopArm;
    public float TopArmRange => _topArmRange;

    public bool HasBottomArm => _hasBottomArm;
    public float BottomArmRange => _bottomArmRange;

    public Queue<NextWalls> NextWallsQueue => new Queue<NextWalls>(_nextWallsQueue);
}