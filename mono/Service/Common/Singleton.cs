using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance{
        get => _instance;
    }

    private void Awake(){
        _instance = this as T; 
          
    }

    


}


