Shader "DepthOnly"
{
    Properties
    {
        
    }

    SubShader
    {
        Tags
		{ 
			"Queue"="Transparent+1"
			"RenderType"="Transparent"
		}

        Pass
        {
            //write to depth buffer but don't draw anything
            ZWrite On
			ColorMask 0

            //transparent materials that rely on this depth shader should render at Transparent+2
        }
    }
}
