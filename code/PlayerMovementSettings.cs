

public static class Settings {

    public static bool HighAirAcceleration { get; set; }
    public static bool LowGravity { get; set; }
    public static bool AutoHop { get; set; }
    public static bool WallJump { get; set; }
    public static bool WallSlide { get; set; }

    static Settings() {

        HighAirAcceleration = true;
        LowGravity = true;
        AutoHop = true;
        WallJump = true;
        WallSlide = true;

    }

}