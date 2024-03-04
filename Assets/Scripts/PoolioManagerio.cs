using System.Collections.Generic;
using UnityEngine;

public class PoolioManagerio : MonoBehaviour
{
    private static PoolioManagerio s_Instance;

    public static PoolioManagerio Instance
    {
        get
        {
            if (s_Instance == null)
            {
                new GameObject().AddComponent<PoolioManagerio>();
            }
            return s_Instance;
        }
    }

    [SerializeField]
    private GameObject _wallParent = null;

    [SerializeField]
    private GameObject _wallPrefab;

    [SerializeField]
    private int _wallPoolCount;

    [SerializeField]
    private GameObject _padCellParent = null;

    [SerializeField]
    private GameObject _padCellPrefab;

    [SerializeField]
    private int _padCellPoolCount;

    [SerializeField]
    private GameObject _columnColliderParent = null;

    [SerializeField]
    private GameObject _columnColliderPrefab;

    [SerializeField]
    private int _columColliderPoolCount;

    [SerializeField]
    private GameObject _armParent = null;

    [SerializeField]
    private GameObject _armPrefab;

    [SerializeField]
    private int _armPoolCount;

    [SerializeField]
    private GameObject _holoFaceParent = null;

    [SerializeField]
    private GameObject _holoFacePrefab;

    [SerializeField]
    private int _holoFacePoolCount;

    private Queue<Wall> _wallPool = new Queue<Wall>();
    private Queue<GameObject> _padCellPool = new Queue<GameObject>();
    private Queue<ColumnCollider> _columnColliderPool = new Queue<ColumnCollider>();
    private Queue<Arm> _armPool = new Queue<Arm>();
    private Queue<GameObject> _holoFacePool = new Queue<GameObject>();

    public GameObject WallPrefab => _wallPrefab;
    public Queue<Wall> WallPool => _wallPool;
    public GameObject WallParent => _wallParent;
    public GameObject PadCellPrefab => _padCellPrefab;
    public Queue<GameObject> PadCellPool => _padCellPool;
    public GameObject PadCellParent => _padCellParent;
    public GameObject ColumnColliderPrefab => _columnColliderPrefab;
    public Queue<ColumnCollider> ColumnColliderPool => _columnColliderPool;
    public GameObject ColumnColliderParent => _columnColliderParent;
    public GameObject ArmPrefab => _armPrefab;
    public Queue<Arm> ArmPool => _armPool;
    public GameObject ArmParent => _armParent;
    public GameObject HoloFacePrefab => _holoFacePrefab;
    public Queue<GameObject> HoloFacePool => _holoFacePool;
    public GameObject HoloFaceParent => _holoFaceParent;

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;

            if (_wallParent == null) _wallParent = new GameObject("Walls");
            for (int i = 0; i < _wallPoolCount; i++)
                _wallPool.Enqueue(Instantiate(_wallPrefab, _wallParent.transform).GetComponent<Wall>());

            if (_padCellParent == null) _padCellParent = new GameObject("Pad Cells");
            for (int i = 0; i < _padCellPoolCount; i++)
                _padCellPool.Enqueue(Instantiate(_padCellPrefab, _padCellParent.transform));

            if (_columnColliderParent == null) _columnColliderParent = new GameObject("Column Colliders");
            for (int i = 0; i < _columColliderPoolCount; i++)
                _columnColliderPool.Enqueue(Instantiate(_columnColliderPrefab, _columnColliderParent.transform).GetComponent<ColumnCollider>());

            if (_holoFaceParent == null) _holoFaceParent = new GameObject("Holo Faces");
            for (int i = 0; i < _holoFacePoolCount; i++)
                _holoFacePool.Enqueue(Instantiate(_holoFacePrefab, _holoFaceParent.transform));
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}