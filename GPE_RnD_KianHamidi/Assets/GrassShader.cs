using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrassShader : MonoBehaviour
{
    //material
    public Material material;
    //slider textmeshpro
    public Slider sliderGrassLenght;
    // Start is called before the first frame update
    void Start()
    {
        sliderGrassLenght.value = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_FurLength", sliderGrassLenght.value);
    }
}
