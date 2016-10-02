using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Music
{
    public static class WinAPI
    {
        public const int MEDIA_NEXT_TRACK = 0xB0;
        public const int MEDIA_PREV_TRACK = 0xB1;
        public const int MEDIA_STOP = 0xB2;
        public const int MEDIA_PLAY_PAUSE = 0xB3;

        public const int INPUT_KEYBOARD = 1;

        [DllImport("User32.dll")]
        public static extern uint SendInput(uint numberOfInputs, ref INPUT input,
        int structSize);

        [DllImport("user32.dll")]
        public static extern uint GetMessageExtraInfo();

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBOARDINPUT
        {
            public uint type;
            public ushort vk;
            public ushort scanCode;
            public uint flags;
            public uint time;
            public uint extrainfo;
            public uint padding1;
            public uint padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public uint dx;
            public uint dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBOARDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        public static void SendKeyPress(ushort key)
        {
            var input = new INPUT();
            input.type = INPUT_KEYBOARD;

            // Key down shift, ctrl, and/or alt
            input.ki.scanCode = 0;
            input.ki.time = 0;
            input.ki.flags = 0;
            input.ki.extrainfo = GetMessageExtraInfo();
            input.ki.vk = key;
            SendInput(1, ref input, Marshal.SizeOf(input));
        }
    }
}
