using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using RVO;
using Vector2 = RVO.Vector2;
public class ObstacleConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] BoxCollider  boxCollider;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

       
        float minX = boxCollider.transform.position.x -
                         boxCollider.size.x*boxCollider.transform.lossyScale.x*0.5f;
        float minZ = boxCollider.transform.position.z -
                        boxCollider.size.z*boxCollider.transform.lossyScale.z*0.5f;
        float maxX = boxCollider.transform.position.x +
                        boxCollider.size.x*boxCollider.transform.lossyScale.x*0.5f;
        float maxZ = boxCollider.transform.position.z +
                        boxCollider.size.z*boxCollider.transform.lossyScale.z*0.5f;
        dstManager.AddBuffer<ObstacleVertice>(entity);
        var vertices =  dstManager.GetBuffer<ObstacleVertice>(entity);
        vertices.Add( new ObstacleVertice{vertice = new Vector2((FixedInt)maxX, (FixedInt)maxZ  )});
        vertices.Add(new ObstacleVertice{vertice = new Vector2((FixedInt)minX, (FixedInt)maxZ  )});
        vertices.Add(new ObstacleVertice{vertice = new Vector2((FixedInt)minX, (FixedInt)minZ  )});
        vertices.Add(new ObstacleVertice{vertice = new Vector2((FixedInt)maxX, (FixedInt)minZ  )});
    }


    // void   ObstacleCollect(){

    //     int obstacleNo = Root.Instance.obstacleCount;  

    //         for (int i = 0; i < vertices.Count; ++i)
    //         {
    //             Obstacle obstacle = new Obstacle();
    //             obstacle.point_ = vertices[i];

    //             if (i != 0)
    //             {
    //                 obstacle.previous_ = obstacles_[obstacleNo - 1];
    //                 obstacle.previous_.next_ = obstacle;
    //             }

    //             if (i == vertices.Count - 1)
    //             {
    //                 obstacle.next_ = obstacles_[obstacleNo];
    //                 obstacle.next_.previous_ = obstacle;
    //             }

    //             obstacle.direction_ = RVOMath.normalize(vertices[(i == vertices.Count - 1 ? 0 : i + 1)] - vertices[i]);

    //             if (vertices.Count == 2)
    //             {
    //                 obstacle.convex_ = true;
    //             }
    //             else
    //             {
    //                 obstacle.convex_ = (RVOMath.leftOf(vertices[(i == 0 ? vertices.Count - 1 : i - 1)], vertices[i], vertices[(i == vertices.Count - 1 ? 0 : i + 1)]) >= 0 );
    //             }

    //             obstacle.id_ = obstacles_.Count;
    //             obstacles_.Add(obstacle);
    //         }

    //         return obstacleNo;


    // }
}
