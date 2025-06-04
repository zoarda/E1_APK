using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string p = Application.absoluteURL;
        Debug.Log($"absoluteURL{p}");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
