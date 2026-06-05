using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [SerializeField] private float lineFadeRate;
    [SerializeField] private float lineShrinkRate;
    private LineRenderer lineRenderer;
    private Color newColor;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        newColor = lineRenderer.material.color;

    }

    // Update is called once per frame
    void Update()
    {
        newColor.a -= lineFadeRate * Time.deltaTime;
        lineRenderer.material.color = newColor;

        lineRenderer.startWidth -= lineShrinkRate * Time.deltaTime;
        lineRenderer.endWidth -= lineShrinkRate * Time.deltaTime;

        if (newColor.a <= 0)
        {
            Destroy(gameObject);
        }


    }
}
