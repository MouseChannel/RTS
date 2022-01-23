using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

    public struct Line :IBufferElementData
    {
        public Vector2 direction;
        public Vector2 point;
    }
