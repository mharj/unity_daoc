using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TerrainTextureCsvLoader {
	public static List<TerrainTextureInfo> read(FileStream file) 
	{
		List<TerrainTextureInfo> ret = new List<TerrainTextureInfo>();
		StreamReader sr = new StreamReader(file);
		sr.ReadLine(); // skip first info line
		string line;
		while((line = sr.ReadLine()) != null)
		{
			line = line.Trim();
			if ( line.Length > 0 ) 
			{
				string[] parts = line.Split(',');
				ret.Add(TerrainTextureInfo.FromArray(parts));
			}
		}
		sr.Close();
		return ret;
	}
}
