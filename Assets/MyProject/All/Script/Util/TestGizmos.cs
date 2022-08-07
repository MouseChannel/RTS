using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FixedMath;

public class TestGizmos : MonoBehaviour
{
    // Start is called before the first frame update
    private static GridNode[] gridArray;
    void Start()
    {
        // gridArray = GridSystem.gridArray;

    }

    // Update is called once per frame
 
    public  static void InitGizmos(){
        for (int i = 0; i < gridArray.Length;i++){
            gridArray[i].isWalkable = true;
        }
    }
    
    public static void SetGridUnvisiable(int2 pos){
        gridArray[pos.y *100 + pos.x].isWalkable = false;
    }
    void Update(){
        // if(Input.GetMouseButtonDown(1)){
        //     for (int i = 0; i < 100;i++)
        //         for (int j = 0; j < 100; j++)
        //         {
        //             var index = GridSystem.GetGridIndex(new FixedVector2(i, j));
        //             gridArray[index].giz = false;
        //         }
        // }
    }
    void OnDrawGizmos(){
         Gizmos.color = Color.green;
            for (int i = 0; i < 100;i++)
                for (int j = 0; j < 100;j++){
                var index = GridSystem.GetGridIndex(new FixedVector2(i, j));
                if(!gridArray[index].giz)
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), Vector3.one);
                else 
                    Gizmos.DrawCube(new Vector3(i, 0, j), Vector3.one);
                    // Gizmos.


                }
    }
}
