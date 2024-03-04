using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSlider : MonoBehaviour
{
    private int _fingerId = -1;

    private Camera _camera;
    private Arm _arm;
    private WallBuildingPad _padController;

    private void Awake()
    {
        _arm = transform.GetChild(0).GetComponent<Arm>();
    }

    private void Start()
    {
        _camera = GameController.Instance.GameCamera;
        _padController = GameController.Instance.GamePad;
        GameController.Instance.AddObjectTouchHandler(TouchPhase.Began, tag, OnTouchBegin);
        GameController.Instance.AddObjectTouchHandler(TouchPhase.Moved, tag, OnTouchMoved);
        GameController.Instance.AddObjectTouchHandler(TouchPhase.Ended, tag, OnTouchEnd);
    }

    private void OnTouchBegin(object sender, ObjectTouchEventArgs args)
    {
        if (args.HitTransform == transform)
            _fingerId = args.ObjectTouch.fingerId;
    }

    private void OnTouchMoved(object sender, ObjectTouchEventArgs args)
    {
        Touch touch = args.ObjectTouch;
        if (_fingerId == touch.fingerId)
        {
            Vector3 touchPos = touch.position;

            touchPos.z = _camera.nearClipPlane + Mathf.Abs(_camera.transform.position.z - transform.transform.position.z);
            if (_arm.Facing == ArmFacing.UP || _arm.Facing == ArmFacing.DOWN)
            {
                transform.Translate((_camera.ScreenToWorldPoint(touchPos).x - transform.position.x), 0f, 0f);

                if (transform.position.x < _padController.PadGrid.GridBounds.min.x)
                    transform.transform.Translate(_padController.PadGrid.GridBounds.min.x - transform.position.x, 0f, 0f);
                else if (transform.position.x > _padController.PadGrid.GridBounds.max.x)
                    transform.transform.Translate(_padController.PadGrid.GridBounds.max.x - transform.position.x, 0f, 0f);
            }
            else
            {
                transform.Translate(0f, (_camera.ScreenToWorldPoint(touchPos).y - transform.position.y), 0f, Space.World);

                if (transform.position.y < _padController.PadGrid.GridBounds.min.y)
                    transform.transform.Translate(0f, _padController.PadGrid.GridBounds.min.y - transform.position.y, 0f, Space.World);
                else if (transform.position.y > _padController.PadGrid.GridBounds.max.y)
                    transform.transform.Translate(0f, _padController.PadGrid.GridBounds.max.y - transform.position.y, 0f, Space.World);
            }
        }
    }

    private void OnTouchEnd(object sender, ObjectTouchEventArgs args)
    {
        if (args.ObjectTouch.fingerId == _fingerId)
            _fingerId = -1;
    }
}