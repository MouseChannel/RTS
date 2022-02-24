using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImMainCanvas : MonoBehaviour
{
     
    void Start()
    {
        ResourceService.Instance.MainCanvas = transform;
    }

 
}
