Shader "Custom/Terrain" {
	Properties {

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static float epsilon = 1E-4;

		const static int maxLayerCount=8;

		int layerCount;
		float3 baseColours[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColourStrength[maxLayerCount];
		float minHeight;
		float maxHeight;

		int layerCountR;
		float3 baseColoursR[maxLayerCount];
		float baseStartHeightsR[maxLayerCount];
		float baseBlendsR[maxLayerCount];
		float baseColourStrengthR[maxLayerCount];
		float minHeightR;
		float maxHeightR;

		int layerCountF;
		float3 baseColoursF[maxLayerCount];
		float baseStartHeightsF[maxLayerCount];
		float baseBlendsF[maxLayerCount];
		float baseColourStrengthF[maxLayerCount];
		float minHeightF;
		float maxHeightF;

		float chunkPosX;
		float chunkPosZ;
		float width;

		int typeOfCorner;

		struct Input {
			float3 worldPos;
			float2 uv_MainTex;
		};

		float inverseLerp(float a, float b, float value){
			return saturate((value-a)/(b-a));
		}

		float3 calculateSelfSurf(Input IN, float3 albedo, float heightPercent){
			for (int i = 0; i < layerCount; i ++) {
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - (baseStartHeights[i] ) );
				albedo = albedo * (1-drawStrength) + baseColours[i]* drawStrength;
			}
			return albedo;
		}

		float3 calculateRightSurf(Input IN, float3 albedo, float heightPercent){
			for (int i = 0; i < layerCountR; i ++) {
				float drawStrength = inverseLerp(-baseBlendsR[i]/2 - epsilon, baseBlendsR[i]/2, heightPercent - (baseStartHeightsR[i] ) );
				albedo = albedo * (1-drawStrength) + baseColoursR[i]* drawStrength;
			}
			return albedo;
		}

		float3 calculateFrontSurf(Input IN, float3 albedo, float heightPercent){
			for (int i = 0; i < layerCountF; i ++) {
				float drawStrength = inverseLerp(-baseBlendsF[i]/2 - epsilon, baseBlendsF[i]/2, heightPercent - (baseStartHeightsF[i] ) );
				albedo = albedo * (1-drawStrength) + baseColoursF[i]* drawStrength;
			}
			return albedo;
		}

		float3 calculateOnlyRightAproxToBiome(  float startCoord,float x, float y, float3 thisHeight, float3 differentBiomeHeight){
			float fixWidth=width-startCoord;

	                    if ( x==(fixWidth-1) ) {
	                              x=fixWidth;
	                    }
	                    float thisMultiplier =   (fixWidth -x + y ) / fixWidth ;
	                    float differentBiomeMultiplier=1-thisMultiplier;
	                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;

	                    return thisHeight;

	          }

		float3 CalculateCornerAproxToBiome( float startCoord, float x, float y, float3 thisHeight, float3 differentBiomeHeight){
			float fixWidth=width-startCoord;
	                    if (x==(fixWidth-1) || y==(fixWidth-1)) {
	                              thisHeight = differentBiomeHeight;
	                    }else {
	                              float thisMultiplier = ( 1 - x/fixWidth ) * ( 1 - y/fixWidth );
	                              thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*(1-thisMultiplier);
	                    }
	                    return thisHeight;
	          }

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float heightPercent = inverseLerp(minHeight,maxHeight, IN.worldPos.y);
			float3 albedo= o.Albedo;

			o.Albedo = calculateSelfSurf( IN,  albedo, heightPercent);


			if(typeOfCorner!=0){
				float heightPercentR = inverseLerp( minHeightR, maxHeightR, IN.worldPos.y);
				float heightPercentF = inverseLerp( minHeightF, maxHeightF, IN.worldPos.y);
				float startCoord =35.5;
				float fixedX =  IN.worldPos.x-chunkPosX+24;
				float fixedZ =  IN.worldPos.z-chunkPosZ+24;

				if(typeOfCorner==1){
					if(fixedX>=startCoord || fixedZ<=width-startCoord){
						if (  fixedX>=startCoord) {
							float3 differentBiomeColor= calculateRightSurf( IN,  albedo, heightPercentR);
							o.Albedo= CalculateCornerAproxToBiome( startCoord, fixedX-startCoord, 0, o.Albedo, differentBiomeColor );
						}
					}

				}
				if(typeOfCorner==5){
					if ( fixedZ>=startCoord) {
						float3 differentBiomeColor= calculateFrontSurf( IN,  albedo, heightPercentF);
						o.Albedo= CalculateCornerAproxToBiome( startCoord, 0, fixedZ-startCoord, o.Albedo, differentBiomeColor );
					}
				}
				if(typeOfCorner==6){
					if (  fixedX>=startCoord) {
						float3 differentBiomeColor= calculateRightSurf( IN,  albedo, heightPercentR);
						o.Albedo= CalculateCornerAproxToBiome( startCoord, fixedX-startCoord, 0, o.Albedo, differentBiomeColor );
					}
				}


			}



		}
		ENDCG
	}

	FallBack "Diffuse"
}
