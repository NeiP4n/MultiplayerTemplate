using Sources.Code.Gameplay.Characters;
using Sources.Code.Gameplay.GameSaves;
using Sources.Controllers;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    private PlayerCharacter  player;
    private CameraController cameraCtrl;

    void Awake()
    {
        player = GetComponentInChildren<PlayerCharacter>();
        cameraCtrl = player.GetComponentInChildren<CameraController>();
    }
    void Reset()
    {
        if (player == null)
            player = GetComponentInChildren<PlayerCharacter>();

        if (cameraCtrl == null && player != null)
            cameraCtrl = player.GetComponentInChildren<CameraController>();
    }

    public void SavePlayer()
    {
        var progress = GameSaverLoader.Instance.PlayerProgress;
        if (progress == null || player == null || cameraCtrl == null)
            return;

        Vector3 pos = player.transform.position;
        progress.PlayerPosX = pos.x;
        progress.PlayerPosY = pos.y;
        progress.PlayerPosZ = pos.z;

        progress.CameraYaw   = cameraCtrl.GetYaw();
        progress.CameraPitch = cameraCtrl.GetPitch();

        GameSaverLoader.Instance.SaveProgress();
    }

    public void LoadPlayer()
    {
        var progress = GameSaverLoader.Instance.PlayerProgress;
        if (progress == null || player == null || cameraCtrl == null)
            return;

        // позиция
        player.transform.position = new Vector3(
            progress.PlayerPosX,
            progress.PlayerPosY,
            progress.PlayerPosZ
        );

        // камера + тело
        cameraCtrl.SetRotationFromSave(progress.CameraYaw, progress.CameraPitch);
    }
}
