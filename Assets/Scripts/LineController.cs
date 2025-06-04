using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    [SerializeField] private float StartWidth, EndWidth;
    private LineRenderer lr;
    private RectTransform[] points;
    public static LineController instance;
    public static LineController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LineController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.startWidth = StartWidth;
        lr.endWidth = EndWidth;
    }
    public void SetUpLine(RectTransform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;
        if (points == null)
            return;
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 worldlr = points[i].position;
            lr.SetPosition(i, worldlr);
        }
    }
}
