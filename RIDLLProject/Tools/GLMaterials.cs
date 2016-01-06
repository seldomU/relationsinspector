using UnityEngine;

namespace RelationsInspector
{
	public class GLMaterials
	{
		public static Material defaultMat;

		static GLMaterials()
		{
			// http://wiki.unity3d.com/index.php/GLDraw
			defaultMat = new Material( "Shader \"Lines/Colored Blended\" {" +
										"SubShader { Pass { " +
										"    Blend SrcAlpha OneMinusSrcAlpha " +
										"    ZWrite Off Cull Off Fog { Mode Off } " +
										"    BindChannels {" +
										"      Bind \"vertex\", vertex Bind \"color\", color }" +
										"} } }" );

			defaultMat.hideFlags = HideFlags.HideAndDontSave;
			defaultMat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
