using UnityEditor;
using UnityEngine;

public class AtlasExporterWindow : EditorWindow
{
	private static Sprite sprite_;
	private static RenderTexture render_texture_;
	private static Material material_;
	private Texture2D texture_;
    
    [MenuItem("Window/AtlasExporter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasExporterWindow));
    }
    
    void OnGUI()
    {
		GUILayout.Label("This is a tool for create texture atlas.");
		if (GUILayout.Button("convert atlas")) {
			Debug.Log("converting..");

			render_texture_ = AssetDatabase.LoadAssetAtPath("Assets/Textures/blit_render_texture.renderTexture", typeof(RenderTexture)) as RenderTexture;
			material_ = AssetDatabase.LoadAssetAtPath("Assets/Materials/blit.mat", typeof(Material)) as Material;
			sprite_ = AssetDatabase.LoadAssetAtPath("Assets/Textures/GUI/square.png", typeof(Sprite)) as Sprite;

			Texture2D tex2d = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite_, true /* getAtlasData */);
			Graphics.SetRenderTarget(render_texture_);
			GL.Clear(true, true, new Color(0, 0, 0, 0));
			Graphics.Blit(tex2d, render_texture_, material_, 0 /* pass */);

			RenderTexture.active = render_texture_;
			int width = render_texture_.width;
			int height = render_texture_.height;
			texture_ = new Texture2D(width, width, TextureFormat.ARGB32, false /* mipmap */);
			texture_.ReadPixels( new Rect(0, 0, width, height), 0, 0);
			texture_.Apply();
			RenderTexture.active = null; //can help avoid errors 
     
			var atlas_path = "/Textures/GUI/atlas.png";
			byte[] bytes;
			bytes = texture_.EncodeToPNG();
			var path = Application.dataPath + atlas_path;
			System.IO.File.WriteAllBytes(path, bytes);

			var rel_path = "Assets" + atlas_path;
			AssetDatabase.ImportAsset(rel_path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
			Debug.Log("done!");
		}

		if (texture_ != null) {
			Graphics.DrawTexture(new Rect(10, 240, 100, 100), texture_);
		}
    }
}

/*
 * End of AtlasExporterWindow.cs
 */
