using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePage : MonoBehaviour
{
    [SerializeField] private RectTransform[] points;
    private void Start()
    {
        LineController line = LineController.Instance;
        line.SetUpLine(points);
    }
}
