using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class textinput : MonoBehaviour
{

    private TouchScreenKeyboard overlayKeyboard;
    public static string inputText = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        }

    }
}
