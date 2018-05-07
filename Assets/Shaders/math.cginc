
float3 mul_quaternion(float4 quat, float3 vec)
{
    float num = quat.x * 2;
    float num2 = quat.y * 2;
    float num3 = quat.z * 2;
    float num4 = quat.x * num;
    float num5 = quat.y * num2;
    float num6 = quat.z * num3;
    float num7 = quat.x * num2;
    float num8 = quat.x * num3;
    float num9 = quat.y * num3;
    float num10 = quat.w * num;
    float num11 = quat.w * num2;
    float num12 = quat.w * num3;
    float3 result;
    result.x = (1 - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
    result.y = (num7 + num12) * vec.x + (1 - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
    result.z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1 - (num4 + num5)) * vec.z;
    return result;
}

float3 mul_quaternion_z(float4 quat, float vec)
{
    float num = quat.x * 2;
    float num2 = quat.y * 2;
    float num3 = quat.z * 2;
    float num4 = quat.x * num;
    float num5 = quat.y * num2;
    float num6 = quat.z * num3;
    float num8 = quat.x * num3;
    float num9 = quat.y * num3;
    float num10 = quat.w * num;
    float num11 = quat.w * num2;
    float3 result;
    result.x = (num8 + num11) * vec;
    result.y = (num9 - num10) * vec;
    result.z = (1 - (num4 + num5)) * vec;
    return result;
}

float norm2_quaternion(float4 q)
{
    return q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w;
}

float4 normalize_quaternion(float4 q)
{
    float Epsilon = 0.00001;
    float len2 = norm2_quaternion(q);
    return len2 > Epsilon ? q*rsqrt(len2) : float4(0, 0, 0, 1);
}

float4 get_look_rotation(float3 dir)
{
    float Epsilon = 0.00001;
    float3 ndir = normalize(dir);
    float3 fwd = float3(0, 0, 1);
    float d = dot(fwd, ndir);
    float d0 = (1+d)*2;
    float sr = d0 > Epsilon ? rsqrt(d0) : 1;
    float3 c = cross(fwd, ndir);
    float4 q = float4(c.x, c.y, c.z, d0*0.5f)*sr;
    return normalize_quaternion(q);
}

float4 inverse_quaternion(float4 q)
{
    return float4(-q.x, -q.y, -q.z, q.w);
}

float4 mul_quaternion(float4 lhs, float4 rhs)
{
    float x = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
    float y = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
    float z = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
    float w = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
    return float4(x, y, z, w);
}
