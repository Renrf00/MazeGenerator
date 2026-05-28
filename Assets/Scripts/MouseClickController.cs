using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    public UnityEvent<Vector3> OnClick;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                clickPosition = hitInfo.point;
                Debug.Log(clickPosition);

                OnClick.Invoke(clickPosition);
            }
        }

        // Debug.DrawLine(transform.position, clickPosition);
        DebugExtension.DebugWireSphere(clickPosition, 0.5f);
    }
}