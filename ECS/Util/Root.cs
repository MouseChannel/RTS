using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Pb;
using Unity.Mathematics;
public class Root : MonoBehaviour
{
    public static Root Instance {
        get;
        set;
    }
    public int id = 0;
 
    void Awake(){
        Instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetMouseButtonDown(1)){      
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     if(Physics.Raycast(ray, out RaycastHit hit)){
                
        //         FixedVector3 endPos = new FixedVector3 ( (FixedInt)hit.point.x, 1,(FixedInt) hit.point.z);
        //         GridInit.Instance.pathfindingGrid.GetXZ(endPos,out int endx, out int endy);
        //         ValidateGridPosition(ref endx, ref endy);
        //         // NetService.Instance.SendMessage(new PbMessage{
        //         //         Cmd = PbMessage.Types.CMD.Test,
    
        //         //         C = endx,
        //         //         D = endy,
        //         //     });
        //     }
        // }
        if(Input.GetKeyDown(KeyCode.Space)){
            NetService.Instance.SendMessage(new PbMessage{
                Cmd = PbMessage.Types.CMD.Login,
                Name = "login",
            });
        }
       
        
    }
        private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, GridInit.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, GridInit.Instance.pathfindingGrid.GetHeight() - 1);
    }
}
