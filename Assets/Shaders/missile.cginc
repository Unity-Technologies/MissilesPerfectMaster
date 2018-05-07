
struct SpawnData
{
    float3 position_;
    int missile_id_;
    float4 rotation_;
    int target_id_;
    int valid_;
    float random_value_;
    float random_value_second_;
};

struct MissileData
{
    float3 position_;
    float spawn_time_;
    float3 omega_;
    float dead_time_;
    float4 rotation_;
    int target_id_;
    float random_value_;
    float dummy0_;
    float dummy1_;
};

struct TargetData
{
    float3 position_;
    float sqr_radius_;
    float dead_time_;
    float dummy0_;
    float dummy1_;
    float dummy2_;
};

struct ResultData
{
    int packed_;
};

struct SortData
{
    int packed_;    // key<<16 | missile_id
};

#define TRAIL_LENGTH    32
