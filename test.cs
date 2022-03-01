using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class test : MonoBehaviour
{
    FOWSystem f;
    public Material m;

    void Start()
    {
        f = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FOWSystem>();
        m = GetComponent<Renderer>().material;

    }

    // Update is called once per frame
    void Update()
    {
        if (f.FogTexture != null)
        {

            m.SetTexture("_MainTex", f.FogTexture);
        }
    }
    void OnDrawGizmos()
    {
      

        Gizmos.color = Color.green;
            for (int i = 0; i < 100;i++)
                for (int j = 0; j < 100;j++){
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), Vector3.one);
                    // Gizmos.


                }
                    
        

    }
}
