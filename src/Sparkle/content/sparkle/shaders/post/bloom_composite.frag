#version 450

layout (set = 0, binding = 0) uniform texture2D fSceneColor;
layout (set = 0, binding = 1) uniform sampler fSceneColorSampler;

layout (set = 1, binding = 0) uniform texture2D fBloomColor;
layout (set = 1, binding = 1) uniform sampler fBloomColorSampler;

layout (location = 0) in vec2 fTexCoords;

layout (location = 0) out vec4 fFragColor;

void main() {
    vec4 sceneColor = texture(sampler2D(fSceneColor, fSceneColorSampler), fTexCoords);
    vec3 bloomColor = texture(sampler2D(fBloomColor, fBloomColorSampler), fTexCoords).rgb;
    
    vec3 color = sceneColor.rgb + bloomColor;
    
    fFragColor = vec4(color, sceneColor.a);
}