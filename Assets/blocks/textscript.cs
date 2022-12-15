using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class textscript : MonoBehaviour
{
    // Start is called before the first frame update
    private TextMeshPro textMesh;
    private Color defaultColor;
    private Dictionary<int, Color> colorMap = new Dictionary<int, Color>();
    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        defaultColor = new Color(0.5f, 0.5f, 0.5f);

        colorMap.Add(2, new Color(0,1,0));
        colorMap.Add(4, new Color(0.2f, 0.5f, 0.2f));
        colorMap.Add(8, new Color(0.3f, 0.8f, 0.1f));
        colorMap.Add(16, new Color(0.6f, 0.6f, 0.3f));
        colorMap.Add(32, new Color(0.9f, 0.3f, 0.4f));
        colorMap.Add(64, new Color(0.9f, 0.4f, 0.6f));
        colorMap.Add(128, new Color(0.1f, 0.92f, 0.5f));
        colorMap.Add(256, new Color(0.6f, 0.3f, 0.6f));
        colorMap.Add(512, new Color(0.3f, 0.55f, 0.3f));
        colorMap.Add(1024, new Color(0.67f, 0.95f, 0.8f));
        colorMap.Add(2048, new Color(0.2f, 0.39f, 0.9f));
        colorMap.Add(4096, new Color(0.0f, 0.99f, 0.93f));
        colorMap.Add(8192, new Color(0.1f, 0.2f, 0.6f));
        colorMap.Add(16384, new Color(0.5f, 0.3f, 0.91f));
        colorMap.Add(32768, new Color(0.9f, 0.1f, 0.1f));
        colorMap.Add(65536, new Color(0.9f, 0.1f, 0.1f));
        colorMap.Add(131072, new Color(0.9f, 0.0f, 0.1f));
        colorMap.Add(262144, new Color(0.9f, 0.11f, 0.23f));
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            int currVal = transform.parent.GetComponent<SquareScript>().getValue();
            textMesh.text = currVal.ToString();

            Color colorToApply = defaultColor;

            if (colorMap.ContainsKey(currVal))
            {
                colorToApply = colorMap[currVal];
            }

            transform.parent.GetComponent<SpriteRenderer>().color = colorToApply;
            //textMesh.transform.position = GetComponentInParent<Canvas>().GetComponentInChildren<SquareScript>().transform.position;
        }
        catch (Exception e)
        {
            Debug.LogError("could not get value from parent");
            textMesh.text = "0"; 
        }
    }
    public Color getColorForInt(int value)
    {
        if (colorMap.ContainsKey(value))
        {
            return colorMap[value];
        }
        
        return defaultColor;
    }
}
