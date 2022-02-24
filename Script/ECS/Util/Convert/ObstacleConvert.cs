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



}
