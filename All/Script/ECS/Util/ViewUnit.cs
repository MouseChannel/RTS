using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;


using RVO;
public class ViewUnit : MonoBehaviour
{
    [HideInInspector] public Entity entity;
    public UnityEngine.Vector2 poss;
    void Start(){
        FightSystem.Instance.allGameobject.Add(this);
    }


    void Update()
    {
        if (entity == Entity.Null) return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var agent = entityManager.GetComponentData<Agent>(entity);
        var pos = agent.position_;
        poss = new UnityEngine.Vector2(pos.x_.RawFloat, pos.y_.RawFloat);
        var dir = agent.velocity_;
        // Debug.Log(dir);
        transform.position = Vector3.Lerp(transform.position, new Vector3(pos.x_.RawFloat, 0, pos.y_.RawFloat), Time.deltaTime * 4);
        if (RVOMath.absSq(dir) > (FixedInt)0.001)
            transform.forward = Vector3.Lerp(transform.forward, new Vector3(dir.x_.RawFloat, 0, dir.y_.RawFloat), Time.deltaTime * 10);

        MonoPhysics();
    }
    void MonoPhysics()
    {

    }


}
