Shader "Custom/ProgressBarShader"
{
    Properties
    {
        [Header(Main Texture)]
        _MainTex ("Sprite Texture", 2D) = "white" {}
        
        [Header(Border Settings)]
        _BorderColor ("Border Color", Color) = (1,1,1,1)
        _BorderRadius ("Border Radius", Range(0, 0.5)) = 0.1
        _BorderThickness ("Border Thickness", Range(0, 0.1)) = 0.02
        _BorderAlpha ("Border Alpha", Range(0, 1)) = 1
        _BorderSmoothness ("Border Smoothness", Range(0.001, 0.1)) = 0.01
        
        [Header(Background Settings)]
        _BackgroundColor ("Background Color", Color) = (0.2, 0.2, 0.2, 1)
        _BackgroundAlpha ("Background Alpha", Range(0, 1)) = 1
        _BackgroundPadding ("Background Padding", Range(0, 0.1)) = 0.01
        
        [Header(Fill Settings)]
        _FillColor ("Fill Color", Color) = (0, 1, 0, 1)
        _FillAlpha ("Fill Alpha", Range(0, 1)) = 1
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _FillPadding ("Fill Padding", Range(0, 0.1)) = 0.02
        _FillRadius ("Fill Radius", Range(0, 0.5)) = 0.08
        [Toggle] _FillHorizontal ("Horizontal Fill", Float) = 1
        [Toggle] _FillVertical ("Vertical Fill", Float) = 0
        
        [Header(Debug)]
        [Toggle] _DebugMode ("Debug Transparency", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "IgnoreProjector" = "True"
        }
        
        // Force correct transparency settings
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ColorMask RGBA
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // Border properties
            float4 _BorderColor;
            float _BorderRadius;
            float _BorderThickness;
            float _BorderAlpha;
            float _BorderSmoothness;
            
            // Background properties
            float4 _BackgroundColor;
            float _BackgroundAlpha;
            float _BackgroundPadding;
            
            // Fill properties
            float4 _FillColor;
            float _FillAlpha;
            float _FillAmount;
            float _FillPadding;
            float _FillRadius;
            float _FillHorizontal;
            float _FillVertical;
            
            // Debug
            float _DebugMode;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            // SDF for rounded rectangle
            float roundedBoxSDF(float2 center, float2 size, float radius)
            {
                return length(max(abs(center) - size + radius, 0.0)) - radius;
            }
            
            // Smooth step with antialiasing
            float smoothEdge(float distance, float smoothness)
            {
                return 1.0 - smoothstep(-smoothness, smoothness, distance);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = uv - 0.5;
                
                // Sample main texture
                fixed4 texColor = tex2D(_MainTex, uv);
                
                // Debug mode - test transparency
                if (_DebugMode > 0.5)
                {
                    // Draw a simple gradient to test transparency
                    float dist = length(center) * 2.0;
                    if (dist > 1.0)
                    {
                        // Outside circle - fully transparent
                        return fixed4(0, 0, 0, 0);
                    }
                    else
                    {
                        // Inside circle - semi-transparent red
                        return fixed4(1, 0, 0, 0.5 * (1.0 - dist));
                    }
                }
                
                // Border (outer rounded rectangle)
                float borderDistance = roundedBoxSDF(center, float2(0.5, 0.5), _BorderRadius);
                float borderMask = smoothEdge(borderDistance, _BorderSmoothness);
                
                // Early exit for pixels outside the border - return fully transparent
                if (borderMask < 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }
                
                // Inner area (inside the border)
                float2 innerSize = float2(0.5 - _BorderThickness, 0.5 - _BorderThickness);
                float innerDistance = roundedBoxSDF(center, innerSize, max(_BorderRadius - _BorderThickness, 0.0));
                float innerMask = smoothEdge(innerDistance, _BorderSmoothness);
                
                // Background area (with padding from border)
                float2 bgSize = float2(0.5 - _BorderThickness - _BackgroundPadding, 0.5 - _BorderThickness - _BackgroundPadding);
                float bgDistance = roundedBoxSDF(center, bgSize, max(_BorderRadius - _BorderThickness - _BackgroundPadding, 0.0));
                float bgMask = smoothEdge(bgDistance, _BorderSmoothness);
                
                // Fill area
                float2 fillBaseSize = float2(0.5 - _BorderThickness - _FillPadding, 0.5 - _BorderThickness - _FillPadding);
                float fillDistance = roundedBoxSDF(center, fillBaseSize, _FillRadius);
                float fillMask = smoothEdge(fillDistance, _BorderSmoothness);
                
                // Apply fill amount clipping
                if (_FillHorizontal > 0.5)
                {
                    // Horizontal fill (left to right)
                    float fillRight = _BorderThickness + _FillPadding + (1.0 - 2.0 * (_BorderThickness + _FillPadding)) * _FillAmount;
                    fillMask *= smoothstep(fillRight + 0.005, fillRight - 0.005, uv.x);
                }
                else if (_FillVertical > 0.5)
                {
                    // Vertical fill (bottom to top)  
                    float fillTop = 1.0 - (_BorderThickness + _FillPadding + (1.0 - 2.0 * (_BorderThickness + _FillPadding)) * _FillAmount);
                    fillMask *= smoothstep(fillTop - 0.005, fillTop + 0.005, uv.y);
                }
                
                // Start with fully transparent
                fixed4 finalColor = fixed4(0, 0, 0, 0);
                
                // Layer 1: Background (only if alpha > 0)
                if (_BackgroundAlpha > 0.001 && bgMask > 0.001)
                {
                    fixed4 bgColor = _BackgroundColor;
                    float bgAlpha = bgColor.a * _BackgroundAlpha * bgMask * borderMask;
                    finalColor.rgb = bgColor.rgb;
                    finalColor.a = bgAlpha;
                }
                
                // Layer 2: Fill (blend on top of background)
                if (_FillAlpha > 0.001 && _FillAmount > 0.001 && fillMask > 0.001)
                {
                    fixed4 fillColor = _FillColor;
                    float fillAlpha = fillColor.a * _FillAlpha * fillMask * borderMask;
                    
                    // Standard alpha blending (non-premultiplied)
                    finalColor.rgb = lerp(finalColor.rgb, fillColor.rgb, fillAlpha);
                    finalColor.a = max(finalColor.a, fillAlpha);
                }
                
                // Layer 3: Border (only the ring)
                float borderRingMask = borderMask * (1.0 - innerMask);
                if (_BorderAlpha > 0.001 && borderRingMask > 0.001)
                {
                    fixed4 borderColor = _BorderColor;
                    float borderAlpha = borderColor.a * _BorderAlpha * borderRingMask;
                    
                    // Standard alpha blending (non-premultiplied)
                    finalColor.rgb = lerp(finalColor.rgb, borderColor.rgb, borderAlpha);
                    finalColor.a = max(finalColor.a, borderAlpha);
                }
                
                // Apply sprite color tint and texture
                finalColor.rgb *= i.color.rgb * texColor.rgb;
                finalColor.a *= i.color.a * texColor.a;
                
                // Clamp alpha to valid range
                finalColor.a = saturate(finalColor.a);
                
                // Return non-premultiplied alpha (Unity handles it)
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}
