using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
    [Flags]
    public enum PhysicsLayer{
        Unit = 1<<0,
        Ground = 1<<1,
        SelectionBox = 1<<2,
        Building  = 1<<3,
        Interactable = 1<<4,
    }
 