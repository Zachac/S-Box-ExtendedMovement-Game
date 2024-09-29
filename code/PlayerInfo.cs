
public static class PlayerInfo
{

    static CharacterController Player;
    static CameraMovement Camera;
    static PlayerInfo()
    {
    }

    public static void SetPlayer(CharacterController player) {
        Player = player;
    }

    public static void SetCamera(CameraMovement camera) {
        Camera = camera;
    }

    public static Vector3 Velocity()
    {
        if (Player == null) {
            return Vector3.Zero;
        }

        return Player.Velocity;
    }

    public static bool IsFirstPerson()
    {
        if (Camera == null) {
            return false;
        }

        return Camera.IsFirstPerson;
    }
}