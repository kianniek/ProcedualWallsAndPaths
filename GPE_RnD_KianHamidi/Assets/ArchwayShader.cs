using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchwayShader : MonoBehaviour
{
    //material
    public Material material;
    //slider textmeshpro
    public Slider sliderArchwayHeight;
    //saved height
    private float savedHeight;
    // Start is called before the first frame update
    void Start()
    {
        sliderArchwayHeight.value = 0.2f;
    }

    private void Awake()
    {
        if (material == null)
        {
            material = GetComponent<Renderer>().material;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (savedHeight != sliderArchwayHeight.value)
        {
            if (material == null)
            {
                material = GetComponent<Renderer>().material;
                return;
            }
            material.SetFloat("_Height", sliderArchwayHeight.value);
            savedHeight = sliderArchwayHeight.value;
        }
    }
}
