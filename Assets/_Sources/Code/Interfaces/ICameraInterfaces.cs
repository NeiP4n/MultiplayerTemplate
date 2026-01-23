using UnityEngine;

namespace Game.Interfaces
{
    public interface ICameraInputProvider
    {
        Vector2 GetLookDelta();
        Vector2 GetMoveInput();
    }
    public interface ICameraTarget
    {
        Vector3 Position { get; }
        Transform HeadBone { get; }
    }
    public interface IFrozenCamera
    {
        float BaseSensitivity { get; }
        void SetFreezeSensitivity(float value);
    }

}