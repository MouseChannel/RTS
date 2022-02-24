using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using FixedMath;
using RVO;
using System;

public class SelectSystem : SystemBase
{
    private Camera mainCamera;
    public bool IsDragging = false;
    public Vector3 mouseStartPos;
    private RectTransform selectionBoxRect;
    private KDTreeSystem kDTreeSystem;
    public List<int> selectedUnits = new List<int>();
    public Vector3 hitpoint;
    public Vector3 singlepoint;
    public float radiuus;
    protected override void OnCreate()
    {
        // DontDestroyOnLoad(this);
        kDTreeSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<KDTreeSystem>();
    }
    // private void OnDestroy()
    // {
    //     // selectedUnits.Dispose();
    // }

    protected override void OnUpdate()
    {

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0) && !IsDragging)
        {
            IsDragging = Vector3.Distance(mouseStartPos, Input.mousePosition) > 25;
        }
        if (IsDragging)
        {
            DrawSelectBox();
        }

        if (Input.GetMouseButtonUp(0))
        {
            DeSelectAll();

            if (IsDragging)
            {
                SelectMultipleUnit();
            }
            else
            {
                SelectSingleUnit();
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (selectedUnits.Count > 0)
                    NetService.Instance.SendMessage(PbTool.MakeMove(hit.point, selectedUnits));
            }
        }



    }



    private void DrawSelectBox()
    {

        if (selectionBoxRect == null)
        {
            ResourceService.Instance.LoadMainWindow("UI/UIMainWindow/SelectionVisualBox", ref selectionBoxRect);
        }
        if (selectionBoxRect == null) return;

        var mouseEndPos = Input.mousePosition;
        selectionBoxRect.position = (mouseStartPos + mouseEndPos) / 2;
        selectionBoxRect.sizeDelta = new UnityEngine.Vector2(Mathf.Abs(mouseStartPos.x - mouseEndPos.x), Mathf.Abs(mouseStartPos.y - mouseEndPos.y));


    }
    private void DeSelectAll()
    {
        selectedUnits.Clear();

        selectionBoxRect.sizeDelta = UnityEngine.Vector2.zero;
    }
    private Vector4 ModifyRect()
    {
        var mouseEndPos = Input.mousePosition;
        float xmin = 0;
        float xmax = 0;
        float ymin = 0;
        float ymax = 0;

        Ray ray = Camera.main.ScreenPointToRay(mouseStartPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            var point = hit.point;
            xmin = xmax = point.x;
            ymin = ymax = point.z;
        }

        ray = Camera.main.ScreenPointToRay(mouseEndPos);
        if (Physics.Raycast(ray, out RaycastHit hit1, 100))
        {
            var point = hit1.point;
            xmin = Mathf.Min(xmin, point.x);
            xmax = Mathf.Max(xmax, point.x);

            ymin = Mathf.Min(ymin, point.z);
            ymax = Mathf.Max(ymax, point.z);
        }


        return new Vector4(xmin, xmax, ymin, ymax);
    }
    private void SelectSingleUnit()
    {



        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(500);

        if (Physics.Raycast(ray, out RaycastHit hit, 500))
        {
            
            RVO.Vector2 position = new RVO.Vector2((FixedInt)hit.point.x, (FixedInt)hit.point.z);
            FixedInt distance = FixedInt.half * 2;
            singlepoint = hit.point;
            int agentNo = -1;
            kDTreeSystem.GetClosestAgent(position, ref distance, ref agentNo, 0);
            if (agentNo != -1)
            {
                selectedUnits.Add(agentNo);
            }



        }



    }

    private void SelectMultipleUnit()
    {
        
        IsDragging = false;
        Vector4 areaRect = ModifyRect();
  
        RVO.Vector2 centerPoint = new RVO.Vector2((FixedInt) (areaRect[0] + areaRect[1]) / 2, (FixedInt)(areaRect[2] + areaRect[3]) / 2);
        FixedInt radius = RVOMath.distance(new RVO.Vector2((FixedInt)areaRect[0], (FixedInt)areaRect[2]), centerPoint);
 

        List<int> tem = new  List<int>( );

        kDTreeSystem.GetAreaAgents(centerPoint, areaRect, radius, 0, tem);
       
        foreach(var i in tem){
            selectedUnits.Add (i);
        }
        
         
    }

 
}
