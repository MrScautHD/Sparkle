#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    if(texelColor.a <= 0.0) {
        discard;
    }
    
    finalColor = texelColor * colDiffuse * fragColor;
}