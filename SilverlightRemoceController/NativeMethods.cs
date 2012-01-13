namespace SilverlightRemoteController
{
    public class NativeMethods
    {
        public const int WM_APPCOMMAND = 0x0319;
        public const int WM_KEYDOWN = 0x0100;

        public const int VK_LEFT = 0x25;
        public const int VK_RIGHT = 0x27;
        public const int WM_GETDLGCODE = 0x0087;
        public const int THBN_CLICKED = 0x1800;

        const int FAPPCOMMAND_MASK = 0xF000;

        public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        public const int APPCOMMAND_MEDIA_PLAY = 46;
        public const int APPCOMMAND_MEDIA_PAUSE = 47;

        public static int GET_APPCOMMAND_LPARAM(int Lparam)
        {
            return (short)(HIWORD(Lparam) & ~FAPPCOMMAND_MASK);
        }

        private static int HIWORD(int lparam)
        {
            return ((lparam >> 16) & 0xffff);
        }
    }
}
