using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MerTechStreamOverlay
{
	public class GlobalKeyboardHook : IDisposable
	{
		// Delegate for the keyboard hook callback
		private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
		private HookProc proc;

		// Keyboard hook handle
		private IntPtr hookId = IntPtr.Zero;

		// Keyboard messages
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;

		// Event for key actions
		public event Action<KeyAction, Keys> KeyActions; // Renamed to KeyActions

		public GlobalKeyboardHook()
		{
			proc = HookCallback;
			hookId = SetHook(proc);
		}

		public void Dispose()
		{
			UnhookWindowsHookEx(hookId);
		}

		private IntPtr SetHook(HookProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				int vkCode = Marshal.ReadInt32(lParam); // Virtual-key code

				switch ((int)wParam)
				{
					case WM_KEYDOWN:
						KeyActions?.Invoke(KeyAction.KeyDown, (Keys)vkCode);
						break;
					case WM_KEYUP:
						KeyActions?.Invoke(KeyAction.KeyUp, (Keys)vkCode);
						break;
				}
			}
			return CallNextHookEx(hookId, nCode, wParam, lParam);
		}

		// Windows API functions
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}

	public enum KeyAction
	{
		KeyDown,
		KeyUp
	}
}
