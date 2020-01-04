using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData {

	public Layer[] layers;

	float savedMinHeight;
	float savedMaxHeight;
	public Material material;

	public void ApplyToMaterial(){

		material.SetInt("layerCount", layers.Length);
		material.SetColorArray ("baseColours", layers.Select(x => x.tint).ToArray());
		material.SetFloatArray ("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
		material.SetFloatArray ("baseBlends", layers.Select(x => x.blendStrength).ToArray());
		material.SetFloatArray ("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());

		UpdateMeshHeights( savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeights( float minHeight, float maxHeight){

		savedMinHeight=minHeight;
		savedMaxHeight=maxHeight;
		
		material.SetFloat("minHeight", minHeight);
		material.SetFloat("maxHeight", maxHeight);
	}

	[System.Serializable]
	public class Layer{
		public Color tint;
		[Range(0,1)]
		public float tintStrength;
		[Range(0,3)]
		public float startHeight;
		[Range(0,1)]
		public float blendStrength;
	}
}
