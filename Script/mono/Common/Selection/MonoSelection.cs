using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
using Vector2 = UnityEngine.Vector2;

public class MonoSelection : MonoBehaviour
{
    Rect selectionBox;
    UnityEngine.Vector2 startPosition;
    public List<int> selectedUnits;
    EntityManager entityManager;
    [SerializeField]
    RectTransform boxVisual;
    void Start()
    {
        DontDestroyOnLoad(this);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    void Update11()
    {
        if(boxVisual == null){
            boxVisual = GameObject.Find("selectionVisualBox").GetComponent<RectTransform>();
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }
        if (Input.GetMouseButton(0))
        {
            selectionBox.xMin = Mathf.Min(Input.mousePosition.x, startPosition.x);
            selectionBox.xMax = Mathf.Max(Input.mousePosition.x, startPosition.x);
            selectionBox.yMin = Mathf.Min(Input.mousePosition.y, startPosition.y);
            selectionBox.yMax = Mathf.Max(Input.mousePosition.y, startPosition.y);
            Vector2 boxStart = startPosition;
            Vector2 boxEnd = Input.mousePosition;

            Vector2 boxCenter = (boxStart + boxEnd) / 2;
            boxVisual.position = boxCenter;

            Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
            boxVisual.sizeDelta = boxSize;
        }
        if (Input.GetMouseButtonUp(0))
        {
            boxVisual.sizeDelta = Vector2.zero;
            selectedUnits.Clear();
            foreach (var i in FightSystem.Instance.allGameobject)
            {
               
                if (selectionBox.Contains(Camera.main.WorldToScreenPoint(i.transform.position)))
                {
                    
                    var me = entityManager.GetComponentData<Agent>(i.entity);
                  
                        Debug.Log(me.GetType());
                
                    
                    selectedUnits.Add(me.id_);
                }
            }
        }
        if(Input.GetMouseButtonDown(1)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit)){
                // if(selectedUnits.Count > 0)
                // NetService.Instance.SendMessage(PbTool.MakeMove(hit.point, selectedUnits));
            }
        }
    }
}
