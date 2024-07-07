#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec2 resolution;
uniform float samples;
uniform float quality;

// Output fragment color
out vec4 finalColor;

void main() {
    vec4 sum = vec4(0);
    vec2 sizeFactor = vec2(1) / resolution * quality;

    // Texel color fetching from texture sampler
    vec4 source = texture(texture0, fragTexCoord);

    int range = int((samples - 1) / 2);

    for (int x = -range; x <= range; x++) {
        for (int y = -range; y <= range; y++) {
            sum += texture(texture0, fragTexCoord + vec2(x, y) * sizeFactor);
        }
    }

    // Calculate final fragment color
    finalColor = ((sum / (samples * samples)) + source) * colDiffuse;
}