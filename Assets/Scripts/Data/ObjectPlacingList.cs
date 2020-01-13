﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ObjectPlacingList : UpdatableData
{
    public ObjectData [] objectsSettings;
    public GameObject waterObj;
    public float waterHeightPos;
    public float minDstWaterGround;

}


[System.Serializable]
public struct ObjectData
{
          public string nombre;
          public GameObject [] modelos;
          public float startHeight;
          public float endHeight;
          public float offsetHeight;
          public float radius;
          public int rejectionSamples;
          public bool randomMaterial;
          public Material [] materiales;
          public bool randomScale;
          public float minScale;
          public float maxScale;
}
