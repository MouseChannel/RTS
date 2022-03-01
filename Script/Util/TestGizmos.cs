using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), Vector3.one);
                    // Gizmos.


                }
    }
}
