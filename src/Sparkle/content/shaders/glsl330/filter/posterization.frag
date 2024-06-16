#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform float gamma;
uniform int numOfColors;

// Output fragment color
out vec4 finalColor;

void main() {
    vec3 texelColor = texture(texture0, fragTexCoord.xy).rgb;

    texelColor = pow(texelColor, vec3(gamma, gamma, gamma));
    texelColor = texelColor * numOfColors;
    texelColor = floor(texelColor);
    texelColor = texelColor / numOfColors;
    texelColor = pow(texelColor, vec3(1.0 / gamma));

    finalColor = vec4(texelColor, 1.0);
}