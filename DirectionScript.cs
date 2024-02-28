using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionScript : MonoBehaviour
{
    private Vector3 mousePos;
    private Vector3 target;

    void Update()
    {
        mousePos = Input.mousePosition;
        target = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x * -10, mousePos.y * -10, -10));
        transform.LookAt(target);
    }
}