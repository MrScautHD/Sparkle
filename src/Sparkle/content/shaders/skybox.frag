#version 330

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;

// Input uniform values
uniform samplerCube environmentMap;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

void main() {
    vec3 color = vec3(0.0);
    
    color = texture(environmentMap, fragPosition).rgb;
    
    finalColor = vec4(color, 1.0) * colDiffuse;
}