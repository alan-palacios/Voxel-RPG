using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator{

          static float minValue;
          static float maxValue;

          public static HeightMap GenerateHeightMap( BiomeData biomeData, Vector2 sampleCentre, BiomesList biomesList){

                    int typeOfCorner = 0;

                    int width = biomeData.meshSettings.noiseSize;
                    int height = width;
                    HeightMapSettings settings = biomeData.heightMapSettings;
                    float chunkSize = biomeData.meshSettings.chunkSize;

                    BiomeData [,] nearbyBiomes = HeightMapGenerator.GenerateBiomeMap(2, 2, biomesList, sampleCentre/chunkSize);
                    float [,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

                    AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

                    minValue = float.MaxValue;
                    maxValue = float.MinValue;

                    //None different biome
                    for (int j=0; j <height; j++) {
                              for (int i=0; i <width; i++) {
                                        values[i,j] *= heightCurve_threadsafe.Evaluate(values[i,j]) * settings.heightMultiplier;
                                        if (values[i,j] > maxValue) {
                                                  maxValue = values[i,j];
                                        }
                                        if (values[i,j] < minValue) {
                                                  minValue = values[i,j];
                                        }
                              }
                    }

                    //edges or corners of biomes
                    if ( !(nearbyBiomes[0,0].nombre == nearbyBiomes[0,1].nombre
                    && nearbyBiomes[0,0].nombre == nearbyBiomes[1,0].nombre
                    && nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre) ) {

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
                                        typeOfCorner = 4;
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
                                        typeOfCorner = 3;
                                        for (int j=0; j <height; j++) {
                                                  for (int i=0; i <width; i++) {
                                                            if ( i > width- j - 2) {
                                                                      differentBiomeHeight = differentDiagonalBiomeNoiseMap[i,j];
                                                                      values [i, j] = CalculateDiagonalAproxToBiome(  width, height, i, j, values[i,j], differentBiomeHeight );
                                                            }
                                                  }
                                        }
                              }

                              //  Right Front different biome
                              if (nearbyBiomes[0,0].nombre == nearbyBiomes[1,1].nombre
                              && nearbyBiomes[0,0].nombre != nearbyBiomes[1,0].nombre
                              && nearbyBiomes[1,0].nombre == nearbyBiomes[0,1].nombre) {
                                        typeOfCorner = 7;
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
                                        typeOfCorner = 13;
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
                                        typeOfCorner = 10;
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
                                        typeOfCorner = 11;
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
                                        typeOfCorner = 12;
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
                                        typeOfCorner = 9;
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
                                        typeOfCorner = 8;
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
                                        typeOfCorner = 5;
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
                                        typeOfCorner = 6;
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
                                        typeOfCorner = 1;
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
                                        typeOfCorner = 2;
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

                    return new HeightMap(values, minValue, maxValue, typeOfCorner);
          }

          public static float CalculateCornerAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

                    if (x==(width-1) || y==(height-1)) {
                              thisHeight = differentBiomeHeight;
                    }else {
                              float thisMultiplier = ( 1f - (float)x/(float)width ) * ( 1f - (float)y/(float)height );
                              float differentBiomeMultiplier=1-thisMultiplier;
                              thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;
                    }
                    return thisHeight;
          }

          public static float CalculateDiagonalAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

                    float thisMultiplier =   (float)(2*width -x -y -1 ) / (float)(width) ;
                    float differentBiomeMultiplier=1-thisMultiplier;
                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;
                    return thisHeight;
          }

          public static float CalculateDiagonalEqualAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

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
          public static float CalculateOnlyRightAproxToBiome( int width, int height, int x, int y, float thisHeight, float differentBiomeHeight){

                    if ( x==(width-1) ) {
                              x=width;
                    }
                    float thisMultiplier =   (float)(width -(x - y) ) / (float)(width) ;
                    float differentBiomeMultiplier=1-thisMultiplier;
                    thisHeight = thisHeight*thisMultiplier + differentBiomeHeight*differentBiomeMultiplier;

                    return thisHeight;

          }


          public static float CalculateNearbyBiomeHeights(int i, int width, AnimationCurve heightCurve_threadsafe, float heightMultiplier, float value, float nearbyHeight){

                    float dividier=(float)i/(float)width;
                    float multiplier=1f-dividier;

                    value *= heightCurve_threadsafe.Evaluate(value) * heightMultiplier;
                    //Debug.Log(value+"->"+nearbyRightHeight+":"+nearbyDiagonalHeight);
                    value = value*multiplier+nearbyHeight*dividier;

                    if (value > maxValue) {
                              maxValue = value;
                    }
                    if (value < minValue) {
                              minValue = value;
                    }

                    return value;
          }

          public static float CalculateNearbyBiomeHeights(int width, int height, int x, int y, float value, float rightNearbyHeight, float frontNearbyHeight, AnimationCurve heightCurve_threadsafe, float heightMultiplier){


                    float multiplier = ( 1f - (float)x/(float)width ) * ( 1f - (float)y/(float)height );
                    heightMultiplier *= 1f;
                    value *= heightCurve_threadsafe.Evaluate(value) * heightMultiplier;

                    float rightMultiplier;
                    float frontMultiplier;

                    if ( x>0 && y>0 ) {
                              rightMultiplier = (1f-multiplier) * (float)x/(float)(x+y);
                              frontMultiplier = (1f-multiplier) * (float)y/(float)(x+y);
                    }else if (x==0 && y>0) {
                              rightMultiplier = 0f;
                              frontMultiplier = (1f-multiplier) * (float)y/(float)(x+y);
                    }else if (x>0 && y==0) {
                              rightMultiplier = (1f-multiplier) * (float)x/((float)x+(float)y);
                              frontMultiplier = 0f;
                    }else{
                              rightMultiplier = 0f;
                              frontMultiplier = 0f;
                    }

                    value = value*multiplier + rightNearbyHeight*rightMultiplier + frontNearbyHeight*frontMultiplier;


                    if (value > maxValue) {
                              maxValue = value;
                    }
                    if (value < minValue) {
                              minValue = value;
                    }

                    return value;
          }


          public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre){

                    float [,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

                    AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

                    float minValue = float.MaxValue;
                    float maxValue = float.MinValue;

                    for (int j=0; j <height; j++) {
                              for (int i=0; i <width; i++) {

                                        values[i,j] *= heightCurve_threadsafe.Evaluate(values[i,j]) * settings.heightMultiplier;
                                        if (values[i,j] > maxValue) {
                                                  maxValue = values[i,j];
                                        }
                                        if (values[i,j] < minValue) {
                                                  minValue = values[i,j];
                                        }

                              }

                    }

                    return new HeightMap(values, minValue, maxValue, 0);
          }

          public static BiomeData[,] GenerateBiomeMap(int width, int height, BiomesList biomesList, Vector2 sampleCentre){
                    float [,] wildNoise = Noise.GenerateNoiseMap(width, height, biomesList.wildSettings.noiseSettings, sampleCentre);
                    float [,] humidityNoise = Noise.GenerateNoiseMap(width, height, biomesList.humiditySettings.noiseSettings, sampleCentre);
                    BiomeData [,] values = new BiomeData[width,height];

                    //creating the biome data
                    for (int i=0; i <width; i++) {
                              for (int j=0; j <height; j++) {
                                        for (int x=0; x<biomesList.biomes.Length; x++) {
                                                  if (wildNoise[i,j] >= biomesList.biomes[x].wildnessStartHeight && humidityNoise[i,j] >= biomesList.biomes[x].humidityStartHeight) {
                                                            values[i,j]=biomesList.biomes[x];
                                                  }
                                        }

                              }
                    }

                    return values;
          }

}

public struct HeightMap {
          public readonly float[,] values;
          public readonly float minValue;
          public readonly float maxValue;
          public readonly int typeOfCorner;

          public HeightMap (float[,] values, float minValue, float maxValue, int typeOfCorner)
          {
                    this.values = values;
                    this.minValue=minValue;
                    this.maxValue=maxValue;
                    this.typeOfCorner=typeOfCorner;
          }
}
