#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform float hatchOffsetY;
uniform float lumThreshold01;
uniform float lumThreshold02;
uniform float lumThreshold03;
uniform float lumThreshold04;

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