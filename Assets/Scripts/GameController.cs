using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using NextWalls = LevelData.NextWalls;

public class GameController : MonoBehaviour
{
    private static GameController s_instance;

    public static GameController Instance => s_instance;

    [SerializeField]
    [Min(10f)]
    private float _maxRayDistance;

    [SerializeField]
    private LayerMask _rayMask;

    [SerializeField]
    private TouchController _touchController;

    [SerializeField]
    private WallBuildingPad _pad;

    [SerializeField]
    private LevelData _levelData;

    private bool _gameRunning = false;

    private Vector3 _camCenterWorldPos;
    private GameObject _wallPrefab;
    private Camera _gameCamera;
    private Transform[] _touchedTransforms;

    private Dictionary<TouchPhase, Dictionary<string, EventHandler<ObjectTouchEventArgs>>> _touchEvents;

    public Vector3 CamCenterWorldPos => _camCenterWorldPos;
    public Camera GameCamera => _gameCamera;
    public WallBuildingPad GamePad => _pad;

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(this);
            return;
        }
        _touchedTransforms = new Transform[10];
        _touchEvents = new Dictionary<TouchPhase, Dictionary<string, EventHandler<ObjectTouchEventArgs>>>();
        _touchEvents.Add(TouchPhase.Began, new Dictionary<string, EventHandler<ObjectTouchEventArgs>>());
        _touchEvents.Add(TouchPhase.Moved, new Dictionary<string, EventHandler<ObjectTouchEventArgs>>());
        _touchEvents.Add(TouchPhase.Ended, new Dictionary<string, EventHandler<ObjectTouchEventArgs>>());

        _gameCamera = Camera.main;
        s_instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Vector3 camTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
        Vector3 camBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        _camCenterWorldPos = (camTopRight + camBottomLeft) / 2f;

        _touchController.OnTouchBegin += OnTouchBegin;
        _touchController.OnTouchMove += OnTouchMove;
        _touchController.OnTouchEnd += OnTouchEnd;

        _wallPrefab = PoolioManagerio.Instance.WallPrefab;
        _pad.SetLevelData(_levelData);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_gameRunning && (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0))
        {
            _gameRunning = true;
            StartCoroutine(SpawnWalls());
        }
    }

    public void AddObjectTouchHandler(TouchPhase phase, string objectTag, EventHandler<ObjectTouchEventArgs> handler)
    {
        if (!_touchEvents.ContainsKey(phase))
            throw new ArgumentException("Touch phase " + phase + " is not handled.");

        Dictionary<string, EventHandler<ObjectTouchEventArgs>> handlers = _touchEvents[phase];
        if (!handlers.ContainsKey(objectTag))
        {
            handlers.Add(objectTag, delegate { });
        }
        //handlers[objectTag] -= handler;
        handlers[objectTag] += handler;
    }

    public void RemoveObjectTouchHandler(TouchPhase phase, string objectTag, EventHandler<ObjectTouchEventArgs> handler)
    {
        if (!_touchEvents.ContainsKey(phase))
            throw new ArgumentException("Touch phase " + phase + " is not handled.");

        Dictionary<string, EventHandler<ObjectTouchEventArgs>> handlers = _touchEvents[phase];
        if (!handlers.ContainsKey(objectTag))
            throw new ArgumentException("There is no handler for the tag " + objectTag);

        handlers[objectTag] -= handler;
    }

    private void OnTouchBegin(object sender, Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _maxRayDistance, _rayMask))
        {
            Dictionary<string, EventHandler<ObjectTouchEventArgs>> handlers = _touchEvents[TouchPhase.Began];
            string objectTag = hit.transform.tag;
            if (handlers.ContainsKey(objectTag))
            {
                handlers[objectTag]?.Invoke(sender, new ObjectTouchEventArgs(touch, hit.transform));
                _touchedTransforms[touch.fingerId] = hit.transform;
            }
        }
    }

    private void OnTouchMove(object sender, Touch touch)
    {
        Dictionary<string, EventHandler<ObjectTouchEventArgs>> handlers = _touchEvents[TouchPhase.Moved];
        if (_touchedTransforms[touch.fingerId] != null &&
            handlers.ContainsKey(_touchedTransforms[touch.fingerId].tag))
        {
            Transform tf = _touchedTransforms[touch.fingerId];
            handlers[tf.tag]?.Invoke(sender, new ObjectTouchEventArgs(touch, tf));
        }
    }

    private void OnTouchEnd(object sender, Touch touch)
    {
        Dictionary<string, EventHandler<ObjectTouchEventArgs>> handlers = _touchEvents[TouchPhase.Ended];
        if (_touchedTransforms[touch.fingerId] != null)
        {
            Transform tf = _touchedTransforms[touch.fingerId];
            _touchedTransforms[touch.fingerId] = null;
            if (handlers.ContainsKey(tf.tag))
            {
                handlers[tf.tag]?.Invoke(sender, new ObjectTouchEventArgs(touch, tf));
            }
        }
    }

    private IEnumerator SpawnWalls()
    {
        Queue<NextWalls> nextQueue = _levelData.NextWallsQueue;
        while (nextQueue.Count > 0)
        {
            NextWalls nextWalls = nextQueue.Dequeue();
            for (int i = 0; i < nextWalls.WallCount; i++)
            {
                if (PoolioManagerio.Instance.WallPool.Count == 0)
                {
                    PoolioManagerio.Instance.WallPool.Enqueue(Instantiate(_wallPrefab).GetComponent<Wall>());
                }
                float spawnTime = nextWalls.SpawnTime > 0 ? nextWalls.SpawnTime : 1;
                float arrivalTime = nextWalls.ArrivalTime > 0 ? nextWalls.ArrivalTime : 1;
                yield return new WaitForSeconds(spawnTime);

                Wall wall = PoolioManagerio.Instance.WallPool.Dequeue();
                wall.gameObject.SetActive(true);
                wall.SetGridController(_pad.PadGrid);
                wall.SetHoleCount(nextWalls.HoleCount);
                wall.SetDifficulty(nextWalls.Difficulty);
                wall.SetDifficultyTolerance(nextWalls.DifficultyTolerance);
                wall.GeneratePattern();
                wall.RB.velocity = new Vector3(0, 0, (_pad.PadGrid.transform.position.z - wall.transform.position.z) / arrivalTime);

                StartCoroutine(HandleWall(wall, arrivalTime));
            }
        }
    }

    private IEnumerator HandleWall(Wall wall, float arrivalTime)
    {
        yield return new WaitForSeconds(arrivalTime);
        IList<Vector2Int> wallHoles = wall.HoleLocations;

        bool patternMatches = true;
        for (int i = 0; i < _pad.PadGrid.GetGridSize().x; i++)
        {
            for (int j = 0; j < _pad.PadGrid.GetGridSize().y; j++)
            {
                if (!_pad.IsHoleFilled(i, j) && wallHoles.Contains(new Vector2Int(i, j)))
                {
                    patternMatches = false;
                }
                else if (_pad.IsHoleFilled(i, j) && !wallHoles.Contains(new Vector2Int(i, j)))
                {
                    patternMatches = false;
                }
            }
        }
        if (patternMatches)
        {
            Debug.Log("Wall matched! :)");
        }
        else
        {
            Debug.Log("Wall didn't match! :(");
        }

        _pad.ResetHoles();
        wall.RB.velocity = Vector2.zero;
        wall.transform.SetPositionAndRotation(_wallPrefab.transform.position, _wallPrefab.transform.rotation);
        Vector3 newScale = wall.transform.localScale;
        newScale.z = _pad.PadGrid.GetCellSize().z;
        wall.transform.localScale = newScale;
        wall.gameObject.SetActive(_wallPrefab.activeSelf);
        PoolioManagerio.Instance.WallPool.Enqueue(wall);
    }
}