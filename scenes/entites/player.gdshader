shader_type canvas_item;

// La couleur à supprimer (cliquable dans l'inspecteur)
uniform vec4 chroma_key : source_color = vec4(0.24, 0.49, 0.34, 1.0);
// La tolérance (si le vert n'est pas parfait)
uniform float limit = 0.1;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    // On calcule la distance entre le pixel actuel et le vert
    float d = distance(col.rgb, chroma_key.rgb);
    
    if (d < limit) {
        col.a = 0.0; // On rend le pixel invisible
    }
    COLOR = col;
}