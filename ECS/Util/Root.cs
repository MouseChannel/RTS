using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
public class Root : MonoBehaviour
{
    public static Root Instance {
        get;
        set;
    }
    public int id = 0;
    public Vector2 mousePosition;
    public int obstacleCount = 0;
    void Awake(){
        Instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit)){
            mousePosition = new Vector2 ( (FixedInt)hit.point.x,(FixedInt) hit.point.z);
        }
       
        
    }
}
