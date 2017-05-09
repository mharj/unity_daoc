using UnityEngine;
using System.Collections;


public class TerrainTextureInfo  {
	public int patch_x;
	public int patch_y;
	public string base_texture_filename;
	float rotate;
	float u_translate;
	float v_translate;
	public float u_scale;
	public float v_scale;
	int visible;
	int num_tiles;
	int tileable;
	int normal_map_filename;
	int specular_tint_color;
	float specular_power;
	int mask_type;
	int mask_window_size;

	public static TerrainTextureInfo FromArray(string[] parts) 
	{
		TerrainTextureInfo ret = new TerrainTextureInfo();
		ret.patch_x = int.Parse(parts[0]);
		ret.patch_y = int.Parse(parts[1]);
		ret.base_texture_filename = parts[2].ToLower();
		ret.rotate = float.Parse(parts[3]);
		ret.u_translate = float.Parse(parts[4]);
		ret.v_translate = float.Parse(parts[5]);
		ret.u_scale = float.Parse(parts[6]);
		ret.v_scale = float.Parse(parts[7]);
		return ret;
	}
}
