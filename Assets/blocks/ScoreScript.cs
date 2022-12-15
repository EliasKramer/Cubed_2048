using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    private TextMeshPro textMeshPro;

    private int value = 0;
    private void Start()
    {
        textMeshPro = GetComponent<TextMeshPro>();
    }
    public void addValue(int givenVale)
    {
        this.value += givenVale;
        textMeshPro.text = value.ToString();
    }

    public string getValue()
    {
        Debug.Log("value is " + value + " string val is " + value.ToString());
        return value.ToString();
    }
}
