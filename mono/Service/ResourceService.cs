using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using Unity.Entities;

public class ResourceService : Singleton<ResourceService>
{
 
    private SelectionSystem selectionSystem;
    
   public void Init(){

   }
   private void Start(){
       selectionSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionSystem>();
   }
   private Action prgCB = null;
   public void AsyncLoadScene(string sceneName, Action<float> loadRate, Action loaded) {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);

        prgCB = () => {
            float progress = sceneAsync.progress;
            loadRate?.Invoke(progress);
            if(progress == 1) {
                loaded?.Invoke();
                prgCB = null;
                sceneAsync = null;
            }
        };
    }

     private void Update() {
        prgCB?.Invoke();

    }

    public Sprite LoadSprite(string path, bool cache = false){
       Sprite sprite = null;
       sprite = Resources.Load<Sprite>(path);
       return sprite;
    }

    public Material LoadMaterial(string path, bool cache = false){
        Material material = null;
       material = Resources.Load<Material>(path);
       return material;
       
    }

    private Texture2D _whiteTexture;

        private  Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, new Color(1,1,1,0.2f));
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

    public Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
    public void OnGUI(){

        if(selectionSystem.IsDragging){
            var rect = GetScreenRect(selectionSystem.mouseStartPos, Input.mousePosition);
            GUI.DrawTexture(rect, WhiteTexture);
        }
        
    }
}
