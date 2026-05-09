shader_type spatial;

uniform sampler2D main_texture;
uniform float texture_scale = 1.0;

varying vec3 world_pos;

void vertex() {
    // Calculate world coordinates by multiplying the vertex by the Model Matrix
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    // Map world coordinates to UV. 
    // Using x and z creates a top-down (planar) projection.
    vec2 world_uv = world_pos.xz * texture_scale;
    vec4 tex_color = texture(main_texture, world_uv);

    float cubic_alpha = smoothstep(0.0, 1.0, COLOR.a);

    // Dither logic
    const float dither_table[16] = float[16](
        0.0625, 0.5625, 0.1875, 0.6875,
        0.8125, 0.3125, 0.9375, 0.4375,
        0.25,   0.75,   0.125,  0.625,
        1.0,    0.5,    0.875,  0.375
    );

    int x = int(FRAGCOORD.x) % 4;
    int y = int(FRAGCOORD.y) % 4;
    float limit = dither_table[x + y * 4];

    if (cubic_alpha < limit) {
        discard;
    }

    // Combine texture color with vertex color
    ALBEDO = tex_color.rgb * COLOR.rgb;
}