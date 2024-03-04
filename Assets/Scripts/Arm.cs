using UnityEngine;

public class Arm : MonoBehaviour
{
    [SerializeField]
    private ArmFacing _facing = ArmFacing.UP;

    [SerializeField]
    private float _maxArmLength = 2.5f;

    public ArmFacing Facing => _facing;

    private Vector3 bravo = Vector3.zero;

    private void Update()
    {
        GridController grid = GameController.Instance.GamePad.PadGrid;
        if (_facing == ArmFacing.DOWN || _facing == ArmFacing.UP)
        {
            bravo.x = transform.position.x;
            bravo.y = grid.GridBounds.center.y - (int)_facing * grid.GridBounds.size.y / 2f;
        }
        else
        {
            bravo.x = grid.GridBounds.center.x - ((int)_facing / 2) * grid.GridBounds.size.x / 2f;
            bravo.y = transform.position.y;
        }
        bravo.z = transform.position.z;
    }

    public void SetArmLength(float length)
    {
        _maxArmLength = length > 0 ? length : 0;
    }

    public bool Extend(Vector2 coords)
    {
        if (!isActiveAndEnabled)
            return false;

        GridController _gridController = GameController.Instance.GamePad.PadGrid;
        Vector2 padAlignedCoords = Vector2.zero;
        if (_facing == ArmFacing.DOWN || _facing == ArmFacing.UP)
        {
            padAlignedCoords.x = transform.position.x;
            padAlignedCoords.y = _gridController.GridBounds.center.y - (int)_facing * _gridController.GridBounds.size.y / 2f;
        }
        else
        {
            padAlignedCoords.x = _gridController.GridBounds.center.x - ((int)_facing / 2) * _gridController.GridBounds.size.x / 2f;
            padAlignedCoords.y = transform.position.y;
        }
        bravo = padAlignedCoords;
        bravo.z = transform.position.z;
        if ((coords - padAlignedCoords).sqrMagnitude > Mathf.Pow(_maxArmLength, 2f)) return false;

        //Do stuff
        return true;
    }

    public bool Extend(float x, float y)
    {
        return Extend(new Vector2(x, y));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(bravo, _maxArmLength);
    }
}

public enum ArmFacing
{
    UP = 1,
    DOWN = -1,
    RIGHT = 2,
    LEFT = -2,
}