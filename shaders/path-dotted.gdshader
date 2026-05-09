shader_type spatial;
render_mode unshaded;

uniform vec3 main_color;

void fragment() {
    float cubic_alpha = smoothstep(0.0, 1.0, COLOR.a);
    ALBEDO = main_color.rgb;
    ALPHA = cubic_alpha * cubic_alpha;
}