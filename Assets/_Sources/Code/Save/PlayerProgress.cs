using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    public int LevelNumber;

    public float PlayerPosX;
    public float PlayerPosY;
    public float PlayerPosZ;

    public float CameraYaw;   // поворот тела по Y
    public float CameraPitch; // поворот камеры по X

    public Dictionary<string, string> ObjectsState;
}
