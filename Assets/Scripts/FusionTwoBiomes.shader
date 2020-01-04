Shader "Custom/TwoBiomesFusion" {
	Properties {

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxLayerCount=8;
		const static float epsilon = 1E-4;

		int layerCount;
		float3 baseColours[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColourStrength[maxLayerCount];

		float minHeight;
		float maxHeight;

		struct Input {
			float3 worldPos;
		};

		float inverseLerp(float a, float b, float value){
			return saturate((value-a)/(b-a));
		}
/*
		float4 ProcessFusion(){
			float[,] differentFrontBiomeNoiseMap = HeightMapGenerator.GenerateHeightMap(
                                        width,  height, nearbyBiomes[0,1].heightMapSettings,
                                        sampleCentre).values;

                              float[,] differentRightBiomeNoiseMap = HeightMapGenerator.GenerateHeightMap(
                                        width,  height, nearbyBiomes[1,0].heightMapSettings,
                                        sampleCentre).values;

                              float[,] differentDiagonalBiomeNoiseMap = HeightMapGenerator.GenerateHeightMap(
                                        width,  height, nearbyBiomes[1,1].heightMapSettings,
                                        sampleCentre).values;

                              float differentBiomeHeight;
                              float rightHeightCalculated;
                              float frontHeightCalculated;

                              //Right Front Diagonal different biome
                              if ( nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && (nearbyBiomes[0,1].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,1].nombre == nearbyBiomes[1,1].nombre)   ) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentRightBiomeNoiseMap[i,j];
                                                            values [i, j] = CalculateCornerAproxToBiome( width, height, i, j, values[i,j], differentBiomeHeight );
                                                  }
                                        }
                              }

                              // Diagonal different biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            if ( i > width- j - 2) {
                                                                      differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateDiagonalAproxToBiome(  width, height, i, j, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }
                              }

                              // Diagonal equal biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[1,0].nombre == nearbyBiomes[0,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentRightBiomeNoiseMap[i,j];
                                                            if ( i <= width - j - 1) {
                                                                      values [i, j] = CalculateDiagonalEqualAproxToBiome(  width, height, i, j, values[i,j], differentBiomeHeight);
                                                            }else{
                                                                      values [i, j] = CalculateDiagonalAproxToBiome(  width, height, i, j, differentBiomeHeight, values[i,j] );
                                                            }
                                                  }
                                        }
                              }

                              // Right, Front two different biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[1,0].nombre != nearbyBiomes[0,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            if ( i <= j ) {
                                                                      differentBiomeHeight = differentFrontBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateOnlyRightAproxToBiome(  height, width , j, i, values[i,j], differentBiomeHeight );
                                                            }else{
                                                                      differentBiomeHeight = differentRightBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateOnlyRightAproxToBiome(  width, height, i, j, values[i,j], differentBiomeHeight );

                                                            }
                                                  }
                                        }
                              }

                              // Right Front, Diagonal two different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,1].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,1].nombre != nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            if ( i <= j ) {
                                                                      frontHeightCalculated = CalculateCornerAproxToBiome(  width, height, i, 0, differentFrontBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], frontHeightCalculated );
                                                            }else{
                                                                      rightHeightCalculated = CalculateCornerAproxToBiome(  width, height, 0, j, differentFrontBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], rightHeightCalculated );

                                                            }
                                                  }
                                        }
                              }

                              //Right , Diagonal  two different biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[1,0].nombre != nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            if ( i <= j ) {
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], differentBiomeHeight );

                                                            }else{
                                                                      rightHeightCalculated = CalculateCornerAproxToBiome(  width, height, 0, j, differentRightBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], rightHeightCalculated );

                                                            }
                                                  }
                                        }
                              }


                              //Front , Diagonal  two different biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,1].nombre != nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            if ( i <= j ) {
                                                                      frontHeightCalculated = CalculateCornerAproxToBiome(  width, height, i, 0, differentFrontBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], frontHeightCalculated );
                                                            }else{
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }
                              }

                              //Right Diagonal, Front  two different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[1,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[1,0].nombre == nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            if ( i <= j ) {
                                                                      frontHeightCalculated = CalculateCornerAproxToBiome(  width, height, i, 0, differentFrontBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], frontHeightCalculated );

                                                                      //values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], frontHeightCalculated );
                                                            }else{
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }
                              }

                              //Front Diagonal, Right two different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,1].nombre == nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,1].nombre != nearbyBiomes[1,0].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentFrontBiomeNoiseMap[i,j];
                                                            if ( i <= j ) {
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], differentBiomeHeight );
                                                            }else{
                                                                      rightHeightCalculated = CalculateCornerAproxToBiome(  width, height, 0, j,  differentRightBiomeNoiseMap[i,j], differentBiomeHeight );
                                                                      values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], rightHeightCalculated );

                                                            }
                                                  }
                                        }
                              }

                              //Front Diagonal different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,1].nombre == nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            values [i, j] = CalculateCornerAproxToBiome(  width, height, 0, j, values[i,j], differentBiomeHeight );
                                                  }
                                        }
                              }

                              //Right Diagonal different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[0,1].nombre
                              && nearbyBiomes[1,0].nombre == nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                            values [i, j] = CalculateCornerAproxToBiome(  width, height, i, 0, values[i,j], differentBiomeHeight );
                                                  }
                                        }
                              }

                              //Right Only different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            if ( i >= j ) {
                                                                      differentBiomeHeight = differentRightBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateOnlyRightAproxToBiome(  width, height, i, j, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }

                              }

                              //Front  Only different biome
                              if (nearbyBiomes[0,0].nombre != nearbyBiomes[0,1].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[1,0].nombre
                              && nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre) {

                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            if ( i <= j ) {
                                                                      differentBiomeHeight = differentFrontBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateOnlyRightAproxToBiome(  height, width , j, i, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }

                              }
                    }

		float CalculateCornerAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

	                    if (x==(width-1) || y==(height-1)) {
	                              thisHeight = differentBiomeHeight;
	                    }else {
	                              float thisMultiplier = ( 1f - (float)x/(float)width ) * ( 1f - (float)y/(float)height );
	                              float differentBiomeMultiplier=1-thisMultiplier;
	                              thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;
	                    }
	                    return thisHeight;
	          }

	          float CalculateDiagonalAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

	                    float thisMultiplier =   (float)(2*width -x -y -1 ) / (float)(width) ;
	                    float differentBiomeMultiplier=1-thisMultiplier;
	                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;
	                    return thisHeight;
	          }

	          float CalculateDiagonalEqualAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

	                    if ( x==(width-1) ) {
	                              x=width;
	                    }
	                    if ( y==(height-1) ) {
	                              y=height;
	                    }
	                    float thisMultiplier =   (float)(width -x - y ) / (float)(width) ;
	                    float differentBiomeMultiplier=1-thisMultiplier;
	                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;

	                    return thisHeight;

	          }
	          float CalculateOnlyRightAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

	                    if ( x==(width-1) ) {
	                              x=width;
	                    }
	                    float thisMultiplier =   (float)(width -(x - y) ) / (float)(width) ;
	                    float differentBiomeMultiplier=1-thisMultiplier;
	                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;

	                    return thisHeight;

	          }*/

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float heightPercent = inverseLerp(minHeight,maxHeight, IN.worldPos.y);

			for (int i = 0; i < layerCount; i ++) {
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - (baseStartHeights[i] ) );
				o.Albedo = o.Albedo * (1-drawStrength) + baseColours[i]* drawStrength;
			}

		}
		ENDCG
	}
	FallBack "Diffuse"
}
