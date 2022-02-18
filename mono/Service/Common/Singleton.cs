using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)Activator.CreateInstance(typeof(T), true);
                (instance as Singleton<T>).InitInstance();
                return instance;
            }
            return instance;
        }
    }
    public virtual void InitInstance()
    {
    }


    // private void Awake(){
    //     _instance = this as T; 

    // }




}


