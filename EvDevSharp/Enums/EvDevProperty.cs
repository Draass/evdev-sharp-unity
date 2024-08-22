namespace EvDevSharp
{
    public enum EvDevProperty
    {
        INPUT_PROP_POINTER = 0x00,
        INPUT_PROP_DIRECT = 0x01,
        INPUT_PROP_BUTTONPAD = 0x02,
        INPUT_PROP_SEMI_MT = 0x03,
        INPUT_PROP_TOPBUTTONPAD = 0x04,
        INPUT_PROP_POINTING_STICK = 0x05,
        INPUT_PROP_ACCELEROMETER = 0x06,
        INPUT_PROP_MAX = 0x1f,
        INPUT_PROP_CNT = (INPUT_PROP_MAX + 1),
    }
}