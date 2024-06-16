#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec2 resolution;

// Output fragment color
out vec4 finalColor;

float offset[3] = float[](0.0, 1.3846153846, 3.2307692308);
float weight[3] = float[](0.2270270270, 0.3162162162, 0.0702702703);

void main() {
    vec3 texelColor = texture(texture0, fragTexCoord).rgb * weight[0];

    for (int i = 1; i < 3; i++) {
        texelColor += texture(texture0, fragTexCoord + vec2(offset[i]) / resolution.x, 0.0).rgb * weight[i];
        texelColor += texture(texture0, fragTexCoord - vec2(offset[i]) / resolution.x, 0.0).rgb * weight[i];
    }

    finalColor = vec4(texelColor, 1.0);
}