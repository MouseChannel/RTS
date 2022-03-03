using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FixedMath;

public class TestGizmos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDrawGizmos(){
         Gizmos.color = Color.green;
            for (int i = 0; i < 100;i++)
                for (int j = 0; j < 100;j++){
                var index = GridSystem.GetGridIndex(new FixedVector2(i, j));
                if(GridSystem.Instance.GetGridArray()[index].isWalkable)
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), Vector3.one);
                else 
                Gizmos.DrawCube(new Vector3(i, 0, j), Vector3.one);
                    // Gizmos.


                }
    }
}
