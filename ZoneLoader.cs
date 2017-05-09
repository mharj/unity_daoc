using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class ZoneLoader  {
	public IniParser myini;
	public TerrainData terrainData;
	public float[,] terrainRaw; // [y,x]
	public List<SplatPrototype> TerrainSplatList = new List<SplatPrototype>();
//	private Texture2D myTexture; // todo: build array
	private Texture2D[] TerrainTextures;
	private Dictionary<string,Texture2D> SplatterTextures = new Dictionary<string,Texture2D>();
	private Dictionary<string,int> TextureIdx = new Dictionary<string,int>();
	private float[,,] alphaMap;
//	var map: float[,,] = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, 2];

	public ZoneLoader(int zoneNumber) 
	{
		IniParser sector = new IniParser("Assets/zones/zone" + zoneNumber.ToString("D3") + "/sector.dat");
		int scalefactor = int.Parse(sector.GetSetting("terrain", "scalefactor"));
		int offsetfactor = int.Parse(sector.GetSetting("terrain", "offsetfactor"));
		int sectorSizeX = int.Parse(sector.GetSetting("sectorsize", "sizex"));
		int sectorSizeY = int.Parse(sector.GetSetting("sectorsize", "sizey"));
		int maxHeight = (scalefactor*256)+(offsetfactor*256);


		Debug.Log("Terrain:"+maxHeight);
		PcxLoader terrain = new PcxLoader("Assets/zones/zone" + zoneNumber.ToString("D3") + "/terrain.pcx");
		PcxLoader offset = new PcxLoader("Assets/zones/zone" + zoneNumber.ToString("D3") + "/offset.pcx");


		terrainRaw = new float[terrain.height,terrain.width]; // build array
		for (int y=0 ; y < terrain.height; y++ ) 
		{
			for ( int x=0; x < terrain.width; x++)
			{
				long h = ((int)terrain.getYX(y,x)*scalefactor) + ((int)offset.getYX(y,x)*offsetfactor);
				int rx = (terrain.width-1-x);
				int ry = (terrain.height-1-y);
				terrainRaw[x,y] = ((float)h/(float)maxHeight);
			}
		}
		// sector = 32x32 grid
		terrainData = new TerrainData ();
		terrainData.heightmapResolution = terrain.width;
//		terrainData.size = new Vector3 (65536, maxHeight, 65536);
		terrainData.size = new Vector3 (655.36f, ((float)maxHeight/100), 655.36f);
		terrainData.baseMapResolution = 4096; // base image
		terrainData.alphamapResolution = ((sectorSizeX>sectorSizeY?sectorSizeX:sectorSizeY)*128);
		terrainData.SetHeights(0,0,terrainRaw);

		Debug.Log("ALPHA:"+terrainData.alphamapWidth+","+ terrainData.alphamapHeight+","+ terrainData.alphamapLayers);

		// read texture mappings
		using (FileStream csvFile = File.OpenRead("Assets/zones/zone" + zoneNumber.ToString("D3") + "/textures.csv"))
		{
			List<TerrainTextureInfo> textureInfoList = TerrainTextureCsvLoader.read(csvFile);


			// load all zone textures
			foreach ( TerrainTextureInfo c in textureInfoList ) 
			{
				if ( ! SplatterTextures.ContainsKey(c.base_texture_filename) ) 
				{
					string fileName = "Assets/zones/TerrainTex/"+c.base_texture_filename+".dds";
					Debug.Log("zone read texture:"+fileName);
					SplatterTextures.Add( c.base_texture_filename , LoadTextureDXT( File.ReadAllBytes(fileName), TextureFormat.DXT1) );

					SplatPrototype newSplat = new SplatPrototype();
					newSplat.texture = SplatterTextures[c.base_texture_filename];
					newSplat.tileSize = new Vector2( c.v_scale*128, c.u_scale*128 );
					newSplat.tileOffset = Vector2.zero;
					// newSplat.normalMap = 2D Texture
					TextureIdx[c.base_texture_filename]=TerrainSplatList.Count; // store texture index
					TerrainSplatList.Add(newSplat);

				}
			}
			terrainData.splatPrototypes = TerrainSplatList.ToArray();
			// init alpha maps
			alphaMap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers]; // init 

			Debug.Log("ALPHA:"+terrainData.alphamapWidth+","+ terrainData.alphamapHeight+","+ terrainData.alphamapLayers);


			for( int sy = 0; sy < sectorSizeY ; sy++ )
			{
				for ( int sx = 0; sx < sectorSizeX; sx++ ) 
				{
					List<TerrainTextureInfo> currentTextureInfo = new List<TerrainTextureInfo>();
					foreach ( TerrainTextureInfo c in textureInfoList ) 
					{
						if ( c.patch_x == sx && c.patch_y == sy )
						{
							currentTextureInfo.Add(c);

						}
					}
					int imageCount =  (int)Math.Ceiling( ((float)currentTextureInfo.Count/3));

					Debug.Log("Texture ["+sx+","+sy+"] count: "+currentTextureInfo.Count+" images:"+imageCount);
					int count = 0;
					int old_value = -1;
					Texture2D patch = null;
					Dictionary<int,int> localIndex = new Dictionary<int,int>();

					foreach ( TerrainTextureInfo c in currentTextureInfo )
					{
						localIndex.Add(count++,TextureIdx[c.base_texture_filename]);
					}
					for ( int i = 0; i < currentTextureInfo.Count; i++ ) 
					{
						int current_image = (int)Math.Floor( ((float)i/3));
						if ( current_image != old_value ) // do load or reload
						{
							string patch_file = "Assets/zones/zone" + zoneNumber.ToString("D3") + "/patch"+ sx.ToString("D2") +sy.ToString("D2")+ "-" + current_image.ToString("D2") + ".dds";
							Debug.Log("Load patch texture:"+patch_file);
							patch = LoadTextureDXT( File.ReadAllBytes(patch_file), TextureFormat.DXT5);
							old_value = current_image;
						}
						int cc = i%3;
						for ( int iy = 0; iy < patch.height; iy++) 
						{
							for ( int ix = 0; ix < patch.width; ix++) 
							{
								int rx = (sx*128)+ix;
								int ry = (sy*128)+iy;
								int rry = terrainData.alphamapHeight - 1 - ry;
								int rrx = terrainData.alphamapWidth - 1 - rx;
								if ( cc == 0 ) 
								{
									alphaMap[rx,ry,localIndex[i]] = patch.GetPixel(ix,iy).r;
								}
								if ( cc == 1 ) 
								{
									alphaMap[rx,ry,localIndex[i]] = patch.GetPixel(ix,iy).g;
								}
								if ( cc == 2 ) 
								{
									alphaMap[rx,ry,localIndex[i]] = patch.GetPixel(ix,iy).b;
								}
							}
						}
					}
				}
			}
			terrainData.SetAlphamaps(0,0,alphaMap);
		}
	}


	public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
	{
		if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
			throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
		
		byte ddsSizeCheck = ddsBytes[4];
		if (ddsSizeCheck != 124)
			throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files
		
		int height = ddsBytes[13] * 256 + ddsBytes[12];
		int width = ddsBytes[17] * 256 + ddsBytes[16];
		
		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);
		
		Texture2D texture = new Texture2D(width, height, textureFormat, false);
		texture.LoadRawTextureData(dxtBytes);
		texture.Apply();
		
		return (texture);
	}
}
