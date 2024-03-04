using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(GridController))]
public class WallBuildingPad : MonoBehaviour
{
    [SerializeField]
    private int _padRows;

    [SerializeField]
    private int _padColumns;

    [SerializeField]
    [Min(1f)]
    private int _offScreenRow;

    [SerializeField]
    [Min(1f)]
    private int _offScreenColumn;

    [SerializeField]
    private float _maxDistance;

    [SerializeField]
    private Transform _armParent;

    [SerializeField]
    private Arm _topArm;

    [SerializeField]
    private Arm _bottomArm;

    [SerializeField]
    private Arm _leftArm;

    [SerializeField]
    private Arm _rightArm;

    [SerializeField]
    [Min(0)]
    private float _armMarginMultiplier;

    [SerializeField]
    private float _armZMargin;

    [SerializeField]
    private Transform _holoTransform;

    private LevelData _levelData;

    private bool[,] _holesFilled;
    private ColumnCollider[] _columnColliders;
    private GameObject[,] _holoFaces = new GameObject[0, 0];

    private GridController _gridController;

    public GridController PadGrid => _gridController;

    private void Awake()
    {
        _gridController = GetComponent<GridController>();
    }

    private void Start()
    {
        string cellTag = _gridController.GetCellTag();
        GameController.Instance.AddObjectTouchHandler(TouchPhase.Began, cellTag, OnCellTouchBegin);

        Initialize();
    }

    private void Initialize()
    {
        _holesFilled = new bool[_padRows, _padColumns];
        _columnColliders = new ColumnCollider[_padColumns + 1];

        ResetHoles();
        _gridController.SetGridSize(_padRows, _padColumns);
        Vector3 centerPos = _gridController.GetCellByRowColumn(_padRows / 2, _padColumns / 2).transform.position;
        Vector3 translateVector = GameController.Instance.CamCenterWorldPos - centerPos;
        translateVector.z = 0;
        transform.Translate(translateVector);
        float margin = _gridController.GetCellSize().x * (_armMarginMultiplier);

        centerPos.x += translateVector.x;
        centerPos.y += translateVector.y;
        centerPos.z -= _armZMargin;

        if (_padRows < _offScreenRow)
        {
            _topArm.transform.parent.position = new Vector3(centerPos.x, _gridController.GetCellByRowColumn(_padRows - 1, 0).transform.position.y + margin, centerPos.z);
            _bottomArm.transform.parent.position = new Vector3(centerPos.x, _gridController.GetCellByRowColumn(0, 0).transform.position.y - margin, centerPos.z);
        }
        else
        {
            _topArm.transform.parent.position = new Vector3(0f, 3.75f, centerPos.z);
            _bottomArm.transform.parent.position = new Vector3(0f, -2.75f, centerPos.z);
        }
        if (_padColumns < _offScreenColumn)
        {
            _leftArm.transform.parent.position = new Vector3(_gridController.GetCellByRowColumn(0, 0).transform.position.x - margin, centerPos.y, centerPos.z);
            _rightArm.transform.parent.position = new Vector3(_gridController.GetCellByRowColumn(0, _padColumns - 1).transform.position.x + margin, centerPos.y, centerPos.z);
        }
        else
        {
            _leftArm.transform.parent.position = new Vector3(-2f, 0.5f, centerPos.z);
            _rightArm.transform.parent.position = new Vector3(2f, 0.5f, centerPos.z);
        }

        //SetupColumnColliders();
        SetupHoloFaces();
    }

    private void SetupColumnColliders()
    {
        Vector3 scale = Vector3.zero;
        scale.x = _gridController.GetCellGap().x;
        scale.y = _gridController.GridBounds.size.y * 2f;
        scale.z = _gridController.GetCellSize().z * 1.2f;
        Vector3 coords = Vector3.zero;
        coords.x = _gridController.GetCellByRowColumn(0, 0).transform.position.x - (_gridController.GetCellSize().x + scale.x) / 2f;
        coords.y = _gridController.GetCellByRowColumn((_padRows / 2), (_padColumns / 2)).transform.position.y;
        coords.z = _gridController.GetCellByRowColumn(0, 0).transform.position.z;

        for (int i = 0; i <= _padColumns; i++)
        {
            ColumnCollider col = PoolioManagerio.Instance.ColumnColliderPool.Dequeue();
            col.Column = i;
            col.transform.position = coords;
            coords.x += _gridController.GetCellSize().x + scale.x;
            col.transform.localScale = scale;
            col.gameObject.SetActive(true);
            _columnColliders[i] = col;
        }
    }

    private void SetupHoloFaces()
    {
        _holoTransform.position = _gridController.GetCellByRowColumn((_padRows / 2), (_padColumns / 2)).transform.position;
        _holoTransform.localScale = _gridController.GridBounds.size;
        _holoTransform.gameObject.SetActive(true);
        _holoFaces = new GameObject[_padRows, _padColumns];

        for (int row = 0; row < _padRows; row++)
        {
            for (int column = 0; column < _padColumns; column++)
            {
                GameObject face = PoolioManagerio.Instance.HoloFacePool.Dequeue();
                Vector3 facePos = _gridController.GetCellByRowColumn(row, column).transform.position;
                facePos.z -= _gridController.GetCellSize().z / 2;
                face.transform.position = facePos;
                face.transform.parent = transform;
                face.SetActive(true);

                _holoFaces[row, column] = face;
            }
        }
    }

    public void SetPadSize(int rows, int columns)
    {
        _padRows = rows > 0 ? rows : 7;
        _padColumns = columns > 0 ? columns : 3;
        _gridController.SetGridSize(rows, columns);
        Reset();
    }

    public void SetPadSize(Vector2Int padSize)
    {
        _padRows = padSize.x > 0 ? padSize.x : 7;
        _padColumns = padSize.y > 0 ? padSize.y : 3;
        _gridController.SetGridSize(padSize);
        Reset();
    }

    public void SetLevelData(LevelData levelData)
    {
        _levelData = levelData;
        _topArm.transform.parent.gameObject.SetActive(_levelData.HasTopArm);
        _topArm.SetArmLength(levelData.TopArmRange);

        _bottomArm.transform.parent.gameObject.SetActive(_levelData.HasBottomArm);
        _bottomArm.SetArmLength(levelData.BottomArmRange);

        _leftArm.transform.parent.gameObject.SetActive(_levelData.HasLeftArm);
        _leftArm.SetArmLength(levelData.LeftArmRange);

        _rightArm.transform.parent.gameObject.SetActive(_levelData.HasRightArm);
        _rightArm.SetArmLength(levelData.RightArmRange);

        SetPadSize(_levelData.PadRows, _levelData.PadColumns);
    }

    public bool IsHoleFilled(int row, int column)
    {
        if (row < 0 || column < 0) throw new ArgumentException("Row and column can't be negative!");
        if (row > _padRows - 1 || column > _padColumns - 1) throw new ArgumentException("Row or column exceeds length.");

        return _holesFilled[row, column];
    }

    public void ResetHoles()
    {
        for (int i = 0; i < _padRows; i++)
        {
            for (int j = 0; j < _padColumns; j++)
            {
                _holesFilled[i, j] = false;
                GridCell cell = _gridController.GetCellByRowColumn(i, j);
                cell.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    private void OnCellTouchBegin(object sender, ObjectTouchEventArgs args)
    {
        Transform hitTransform = args.HitTransform;

        GridCell cell = _gridController.GetCellByTransform(hitTransform);
        if (_topArm.Extend(cell.transform.position) || _bottomArm.Extend(cell.transform.position) ||
            _leftArm.Extend(cell.transform.position) || _rightArm.Extend(cell.transform.position))
        {
            Vector2Int cellCoords = cell.GetCellCoords();
            _holesFilled[cellCoords.x, cellCoords.y] = !_holesFilled[cellCoords.x, cellCoords.y];
#if UNITY_EDITOR
            Color col = cell.GetComponent<Renderer>().material.color == Color.red ? Color.green : Color.red;
            cell.GetComponent<Renderer>().material.color = col;
#endif
        }
    }

    public void Reset()
    {
        _holesFilled = null;

        foreach (ColumnCollider col in _columnColliders)
        {
            PoolioManagerio.Instance.ColumnColliderPool.Enqueue(col);
        }

        _holoTransform.gameObject.SetActive(false);
        foreach (GameObject face in _holoFaces)
        {
            face.SetActive(false);
            face.transform.position = PoolioManagerio.Instance.HoloFacePrefab.transform.position;
            face.transform.localScale = PoolioManagerio.Instance.HoloFacePrefab.transform.localScale;
            face.transform.rotation = PoolioManagerio.Instance.HoloFacePrefab.transform.rotation;
            face.transform.parent = PoolioManagerio.Instance.HoloFaceParent.transform;

            PoolioManagerio.Instance.HoloFacePool.Enqueue(face);
        }
        _columnColliders = null;

        _gridController.Reset();

        Initialize();
    }
}