using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Rigidbody))]
public class Wall : MonoBehaviour
{
    private static Random s_wallRandom = new Random();
    private static float s_wallMargin = 0.15f;

    private Rigidbody _rb;
    private GridController _gridController;

    private int _holeCount = 0;
    private float _difficulty = 0f;
    private float _difficultyTolerance = 0f;

    private int _rerollLimit = 10;

    private List<Vector2Int> _holeLocations = new();

    public Rigidbody RB => _rb;

    public IList<Vector2Int> HoleLocations => _holeLocations.AsReadOnly();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void SetGridController(GridController controller)
    {
        _gridController = controller;
        int maxHoles = controller.GetGridSize().x * controller.GetGridSize().y;
        _holeCount = _holeCount <= maxHoles ? _holeCount : maxHoles;

        Vector3 scale = controller.GridBounds.size;
        scale.x += s_wallMargin;
        scale.y += s_wallMargin;
        scale.z = transform.localScale.z;
        transform.localScale = scale;
    }

    public void SetHoleCount(int holeCount)
    {
        _holeCount = holeCount;
    }

    public void SetDifficulty(float difficulty)
    {
        _difficulty = difficulty;
    }

    public void SetDifficultyTolerance(float tolerance)
    {
        _difficultyTolerance = tolerance;
    }

    public void GeneratePattern()
    {
        _holeLocations.Clear();

        Vector2Int gridSize = _gridController.GetGridSize();

        for (int i = 0; i < _holeCount; i++)
        {
            if (_holeLocations.Count > 0)
            {
                bool gotCell = false;
                int rerollCount = 0;

                GridCell topLeftCell = _gridController.GetCellByRowColumn(0, 0);
                Vector2 topLeftCellCoords = topLeftCell.transform.position;
                topLeftCellCoords.x -= topLeftCell.Collider.size.x / 2;
                topLeftCellCoords.y += topLeftCell.Collider.size.y / 2;

                GridCell bottomRightCell = _gridController.GetCellByRowColumn(gridSize.x - 1, gridSize.y - 1);
                Vector2 bottomRightCellCoords = bottomRightCell.transform.position;
                bottomRightCellCoords.x += bottomRightCell.Collider.size.x / 2;
                bottomRightCellCoords.y -= bottomRightCell.Collider.size.y / 2;

                float maxDistanceSquared = Mathf.Pow(topLeftCellCoords.y - bottomRightCellCoords.y, 2) + Mathf.Pow(topLeftCellCoords.x - bottomRightCellCoords.x, 2);

                Vector2Int previousCell = _holeLocations[0];
                Vector3 previousCoords = _gridController.GetCellByRowColumn(previousCell.x, previousCell.y).transform.position;

                while (!gotCell)
                {
                    Vector2Int newCell = new Vector2Int(s_wallRandom.Next(0, gridSize.x), s_wallRandom.Next(0, gridSize.y));
                    Vector3 newCoords = _gridController.GetCellByRowColumn(newCell.x, newCell.y).transform.position;

                    float distanceSquared = Mathf.Pow(newCoords.x - previousCoords.x, 2) + Mathf.Pow(newCoords.y - previousCoords.y, 2);
                    float currDifficulty = distanceSquared * 100 / maxDistanceSquared;

                    if ((_difficulty - _difficultyTolerance < currDifficulty &&
                        currDifficulty < _difficulty + _difficultyTolerance) ||
                        rerollCount < _rerollLimit &&
                        !_holeLocations.Contains(newCell))
                    {
                        _holeLocations.Add(newCell);
                        gotCell = true;

                        //höle test
                        GameObject höle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(höle.GetComponent<BoxCollider>());
                        höle.transform.parent = transform;
                        Vector3 fuck = newCoords;
                        fuck.z = transform.position.z;
                        höle.transform.position = fuck;
                        höle.GetComponent<Renderer>().material.color = Color.blue;
                    }
                    rerollCount++;
                }
            }
            else
            {
                Vector2Int newCell = new Vector2Int(s_wallRandom.Next(0, gridSize.x), s_wallRandom.Next(0, gridSize.y));
                _holeLocations.Add(newCell);

                //höle test
                Vector3 coords = _gridController.GetCellByRowColumn(newCell.x, newCell.y).transform.position;
                GameObject höle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(höle.GetComponent<BoxCollider>());
                höle.transform.parent = transform;
                Vector3 fuck = coords;
                fuck.z = transform.position.z;
                höle.transform.position = fuck;
                höle.GetComponent<Renderer>().material.color = Color.blue;
            }
        }

        DebugStuff();
    }

    private void DebugStuff()
    {
        bool[,] holes = new bool[_gridController.GetGridSize().x, _gridController.GetGridSize().y];
        for (int i = 0; i < _holeLocations.Count; i++)
        {
            holes[_holeLocations[i].x, _holeLocations[i].y] = true;
        }
        string message = "";
        for (int i = _gridController.GetGridSize().x - 1; i >= 0; i--)
        {
            for (int j = 0; j < _gridController.GetGridSize().y; j++)
            {
                message += "[";
                message += holes[i, j] ? "X" : " ";
                message += "]";
            }
            message += "\n";
        }
        Debug.Log(message);
    }
}