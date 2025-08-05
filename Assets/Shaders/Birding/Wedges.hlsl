#ifndef WEDGES
#define WEDGES

// Sets Out to a mask for a circular wedge shape (antialiased).
// center: center of the circle (usually float2(0.5, 0.5))
// radius: outer radius of the wedge
// angleStart: starting angle of the wedge (in radians)
// angleEnd: ending angle of the wedge (in radians)
void CircularWedge_float(float2 UV, float2 center, float radius, float angleStart, float angleEnd, out float4 Out)
{
    float2 dir = UV - center;
    float dist = length(dir);
    float theta = atan2(dir.y, dir.x);

    // Ensure angle is in [0, 2*PI]
    if (theta < 0) theta += 6.2831853;

    // Handle wedge that crosses 0 radians
    bool inAngle = (angleStart < angleEnd) ? (theta >= angleStart && theta <= angleEnd)
                                           : (theta >= angleStart || theta <= angleEnd);

    float mask = (dist <= radius && inAngle) ? 1.0 : 0.0;
    float aa = fwidth(dist);
    float edge = saturate((radius - dist) / aa);

    Out = float4(mask * edge, mask * edge, mask * edge, 1.0);
}

// Cuts a wedge from the center out of a UV (antialiased).
// center: center of the wedge (usually float2(0.5, 0.5))
// angleStart: starting angle of the wedge (in radians)
// angleEnd: ending angle of the wedge (in radians)
void WedgeCutout_float(float2 UV, float2 center, float angleStart, float angleEnd, out float4 Out)
{
    float2 dir = UV - center;
    float theta = atan2(dir.y, dir.x);
    if (theta < 0) theta += 6.2831853;

    float angleMask;
    if (angleStart < angleEnd)
        angleMask = step(angleStart, theta) * step(theta, angleEnd);
    else
        angleMask = step(angleStart, theta) + step(theta, angleEnd);

    angleMask = saturate(angleMask);

    // Antialias the wedge edge using the angular gradient
    float grad = abs(ddx(theta)) + abs(ddy(theta));
    float edge = saturate(angleMask / max(grad, 1e-5));

    Out = float4(edge, edge, edge, 1.0);
}

#endif