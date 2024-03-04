using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTouchEventArgs : EventArgs
{
    public ObjectTouchEventArgs()
    { }

    public ObjectTouchEventArgs(Touch touch, Transform hitTransform)
    {
        ObjectTouch = touch;
        HitTransform = hitTransform;
    }

    public Touch ObjectTouch { get; set; }
    public Transform HitTransform { get; set; }
}