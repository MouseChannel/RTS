using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using Unity.Mathematics;
using FixedMath;

public class Move : MonoBehaviour
{
    Entity entity;
    [SerializeField] Convert c;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        entity = c.entity;
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var agent = entityManager.GetComponentData<Agent>(entity);
        var pos = agent.position_;
        var dir = agent.velocity_;
        // Debug.Log(dir);
        transform.position = Vector3.Lerp(transform.position,new Vector3(pos.x_.RawFloat,0,pos.y_.RawFloat), Time.deltaTime * 4 )  ;
        if(RVOMath.absSq(dir) > (FixedInt)0.001)
            transform.forward = Vector3.Lerp(transform.forward, new Vector3(dir.x_.RawFloat,0,dir.y_.RawFloat) , Time.deltaTime * 10 )  ;
        if(Input.GetMouseButtonDown(0)){
             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit)){
                FixedVector3 endPos = new FixedVector3((FixedInt)hit.point.x,1,(FixedInt)hit.point.z);
                // FixedVector3 endPos = new FixedVector3(13,1,27);
                
                FixedVector3 startPos = new FixedVector3((FixedInt)transform.position.x,1,(FixedInt)transform.position.z);
                
                GridInit.Instance.pathfindingGrid.GetXZ(endPos,out int endx, out int endy);
                ValidateGridPosition(ref endx, ref endy);
                // Debug.Log(endx + " "+endy);
                GridInit.Instance.pathfindingGrid.GetXZ(startPos,out int startx, out int starty);
                ValidateGridPosition(ref startx, ref starty);

              
                entityManager.AddComponentData(entity,  new PathFindParams { 
			    startPosition = new int2(startx, starty), 
                endPosition = new int2(endx, endy) 
                } );
            }
        }
  

        // PathFollow pathFollow = entityManager.GetComponentData<PathFollow>(entity);
        // DynamicBuffer<PathPosition> pathPositionBuffer = entityManager.GetBuffer<PathPosition>(entity);
        // if (pathFollow.pathIndex >= 0) {
        //     // Has path to follow
        //     PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];

        //     float3 targetPosition = new float3(pathPosition.position.x, 0, pathPosition.position.y);
        //     float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);
            
        //     // float moveSpeed = 3f;
        //     agent.prefVelocity_ = new Vector2((FixedInt)moveDir.x, (FixedInt)moveDir.z);

             

        //     // transform.position +=  (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        //     if (math.distance(transform.position, targetPosition) < .1f) {
        //         // Next waypoint
        //         pathFollow.pathIndex--;
        //         entityManager.SetComponentData(entity, pathFollow);
        //     }
        // }


    }
    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, GridInit.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, GridInit.Instance.pathfindingGrid.GetHeight() - 1);
    }


    
}
