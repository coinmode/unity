using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PrintUnderMouse : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<RaycastResult> results = RaycastMouse();
            if(results.Count > 0)
            {
                Debug.Log(results[0].gameObject.name);
            }
        }
    }

    public List<RaycastResult> RaycastMouse()
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
    }
}
