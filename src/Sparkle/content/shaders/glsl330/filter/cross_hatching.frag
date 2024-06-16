#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform float hatchOffsetY = 5.0;
uniform float lumThreshold01 = 0.9;
uniform float lumThreshold02 = 0.7;
uniform float lumThreshold03 = 0.5;
uniform float lumThreshold04 = 0.3;

// Output fragment color
out vec4 finalColor;

void main() {
    vec3 tc = vec3(1.0, 1.0, 1.0);
    float lum = length(texture(texture0, fragTexCoord).rgb);

    if (lum < lumThreshold01) {
        if (mod(gl_FragCoord.x + gl_FragCoord.y, 10.0) == 0.0) {
            tc = vec3(0.0, 0.0, 0.0);
        }
    }

    if (lum < lumThreshold02) {
        if (mod(gl_FragCoord.x - gl_FragCoord.y, 10.0) == 0.0) {
            tc = vec3(0.0, 0.0, 0.0);
        }
    }

    if (lum < lumThreshold03) {
        if (mod(gl_FragCoord.x + gl_FragCoord.y - hatchOffsetY, 10.0) == 0.0) {
            tc = vec3(0.0, 0.0, 0.0);
        }
    }

    if (lum < lumThreshold04) {
        if (mod(gl_FragCoord.x - gl_FragCoord.y - hatchOffsetY, 10.0) == 0.0) {
            tc = vec3(0.0, 0.0, 0.0);
        }
    }

    finalColor = vec4(tc, 1.0);
}