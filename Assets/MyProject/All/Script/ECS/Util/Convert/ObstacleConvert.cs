using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

using Unity.Mathematics;

public class ObstacleConvert :MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] BoxCollider boxCollider;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float x = boxCollider.transform.position.x;
        float y = boxCollider.transform.position.z;

        float minX = boxCollider.transform.position.x -
                         boxCollider.size.x * boxCollider.transform.lossyScale.x * 0.5f;
        float minZ = boxCollider.transform.position.z -
                        boxCollider.size.z * boxCollider.transform.lossyScale.z * 0.5f;
        float maxX = boxCollider.transform.position.x +
                        boxCollider.size.x * boxCollider.transform.lossyScale.x * 0.5f;
        float maxZ = boxCollider.transform.position.z +
                        boxCollider.size.z * boxCollider.transform.lossyScale.z * 0.5f;

        var vertices = dstManager.AddBuffer<PreObstacleVertice>(entity);
        // var obstacleCount = GameData.obstacleCount;
        vertices.Add(new PreObstacleVertice { vertice = new FixedVector2((FixedInt)maxX, (FixedInt)maxZ) });
        vertices.Add(new PreObstacleVertice { vertice = new FixedVector2((FixedInt)minX, (FixedInt)maxZ) });
        vertices.Add(new PreObstacleVertice { vertice = new FixedVector2((FixedInt)minX, (FixedInt)minZ) });
        vertices.Add(new PreObstacleVertice { vertice = new FixedVector2((FixedInt)maxX, (FixedInt)minZ) });
        dstManager.AddComponentData<Obstacle>(entity, new Obstacle { position_ = new FixedVector2((FixedInt)x, (FixedInt)y), id_ = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ResponseNetSystem>().allObstacle.Count });
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ResponseNetSystem>().allObstacle.Add(entity);
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<KDTreeSystem>().UpdateObstacle();
        Debug.Log("绕太阳会让他为" + GridSystem.GetGridIndex(new FixedVector2((FixedInt)minX, (FixedInt)minZ)));

        #region SetPathFinding UnWalkable
        // GridSystem.Instance.SetUnWalkableArea(new int2( (int)boxCollider.transform.position.x, (int)boxCollider.transform.position.z),
        //                                         (FixedInt)(boxCollider.transform.lossyScale.x / 2),4);


        #endregion


    }



}
