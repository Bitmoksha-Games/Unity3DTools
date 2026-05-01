// Extended discussion on this function can be found at the following link:
// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
// Just for fun, some search on the origins got me here: 
// https://stackoverflow.com/questions/12964279/whats-the-origin-of-this-glsl-rand-one-liner
// Couldn't figure out a good reasoning for the logic for the specific constants prevalent everywhere.
// Basic logic seems to be:
// - take the dot product of the float3 (position) with a larger vector.
// - use sin to map the value from -1 to 1
// - multiply by a larger number for more variance.
// - get the fraction part to get a value in the 0..1 range
float rand(float3 co)
{
    // changed the multipliers just because...
    return frac(sin(dot(co.xyz, float3(12.9898 * 2, 78.233 * 3, 53.539 * 4))) * 43758.5453);
}

// Construct a rotation matrix that rotates around the provided axis, sourced from:
// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
// Derivation and explanation: 
// Section 9.2 in https://repository.lboro.ac.uk/articles/thesis/Modelling_CPV/9523520?file=17151500 as 
// mentioned here: https://ai.stackexchange.com/questions/14041/how-can-i-derive-the-rotation-matrix-from-the-axis-angle-rotation-vector
// ... or here: https://en.wikipedia.org/wiki/Rotation_matrix#Rotation_matrix_from_axis_and_angle
float3x3 AngleAxis3x3(float angle, float3 axis)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
}

