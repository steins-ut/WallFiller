using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridController : MonoBehaviour
{
    private Grid _grid;

    [SerializeField]
    [Min(1)]
    private int _gridRows;

    [SerializeField]
    [Min(1)]
    private int _gridColumns;

    [SerializeField]
    private GameObject _cellPrefab;

    private Bounds _gridBounds;
    private GridCell[,] _cells = new GridCell[0, 0];
    private Dictionary<Transform, GridCell> _cellDict = new();

    public Bounds GridBounds => _gridBounds;

    private void Awake()
    {
        _grid = GetComponent<Grid>();
        Initialize();
    }

    private void Initialize()
    {
        _cells = new GridCell[_gridRows, _gridColumns];
        for (int i = 0; i < _gridRows; i++)
        {
            for (int k = 0; k < _gridColumns; k++)
            {
                GridCell cell = Instantiate(_cellPrefab, transform).GetComponent<GridCell>();
                cell.transform.localPosition = _grid.GetCellCenterLocal(new Vector3Int(i - (_gridRows / 2), k - (_gridColumns / 2), 0));
                cell.SetCellCoordinates(i, k);
                _cellDict.Add(cell.transform, cell);
                _cells[i, k] = cell;

#if UNITY_EDITOR
                cell.GetComponent<Renderer>().enabled = true;
                cell.GetComponent<Renderer>().material.color = Color.red;
#endif
            }
        }

        Vector3 size = Vector3.zero;
        size.x = _grid.cellGap.x * (_gridColumns - 1) + _grid.cellSize.x * _gridColumns;
        size.y = _grid.cellGap.y * (_gridRows - 1) + _grid.cellSize.y * _gridRows;
        size.z = _grid.cellSize.z; ;

        _gridBounds = new Bounds(_cells[_gridRows / 2, _gridColumns / 2].transform.position, size);
    }

    public string GetCellTag()
    {
        return _cellPrefab.tag;
    }

    public GridCell GetCellByRowColumn(int row, int column)
    {
        if (row < 0 || column < 0) throw new ArgumentException("Row and column can't be negative!");
        if (row > _gridRows - 1 || column > _gridColumns - 1) throw new ArgumentException("Row or column exceeds length.");

        return _cells[row, column];
    }

    public bool HasCell(Transform transform)
    {
        return _cellDict.ContainsKey(transform);
    }

    public Vector2Int GetGridSize()
    {
        return new Vector2Int(_gridRows, _gridColumns);
    }

    public void SetGridSize(int rows, int columns)
    {
        _gridRows = rows;
        _gridColumns = columns;
        Reset();
    }

    public void SetGridSize(Vector2Int gridSize)
    {
        _gridRows = gridSize.x;
        _gridColumns = gridSize.y;
        Reset();
    }

    public GridCell GetCellByTransform(Transform transform)
    {
        if (!_cellDict.ContainsKey(transform)) throw new ArgumentException("There is no cell with this transform.");

        return _cellDict[transform];
    }

    public Vector2 GetCellGap()
    {
        return _grid.cellGap;
    }

    public Vector3 GetCellSize()
    {
        return _grid.cellSize;
    }

    public void Reset()
    {
        foreach (GridCell cell in _cells)
        {
            Destroy(cell.gameObject);
        }
        _cells = null;
        _cellDict.Clear();
        Initialize();
    }
}