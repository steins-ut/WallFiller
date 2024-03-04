using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    private GridController _gridController;
    private BoxCollider _collider;

    private Vector2Int _gridPos = Vector2Int.zero;

    public BoxCollider Collider => _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public void SetCellCoordinates(int row, int column)
    {
        _gridPos.Set(row, column);
    }

    public Vector2Int GetCellCoords()
    {
        return _gridPos;
    }
}