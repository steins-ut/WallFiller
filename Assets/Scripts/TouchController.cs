using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public event EventHandler<Touch> OnTouchBegin;

    public event EventHandler<Touch> OnTouchMove;

    public event EventHandler<Touch> OnTouchEnd;

    public bool FirstTouchOnly = true;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            int count = FirstTouchOnly ? 1 : Input.touchCount;

            for (int i = 0; i < count; i++)
            {
                Touch touch = Input.GetTouch(i);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegin?.Invoke(this, touch);
                        break;

                    case TouchPhase.Moved:
                        OnTouchMove?.Invoke(this, touch);
                        break;

                    case TouchPhase.Ended:
                        OnTouchEnd?.Invoke(this, touch);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}