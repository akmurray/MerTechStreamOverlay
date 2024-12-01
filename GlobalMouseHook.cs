using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MerTechStreamOverlay
{
	public class GlobalMouseHook : IDisposable
	{
		// Delegate for the mouse hook callback
		private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
		private HookProc proc;

		// Mouse hook handle
		private IntPtr hookId = IntPtr.Zero;

		// Mouse messages
		private const int WH_MOUSE_LL = 14;
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_RBUTTONDOWN = 0x0204;
		private const int WM_RBUTTONUP = 0x0205;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int WM_MBUTTONUP = 0x0208;
		private const int WM_MOUSEWHEEL = 0x020A;
		private const int WM_MOUSEMOVE = 0x0200;

		public event Action<MouseAction> MouseEvent;

		public GlobalMouseHook()
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
				return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				MouseAction? action = null;

				switch ((int)wParam)
				{
					case WM_LBUTTONDOWN:
						action = MouseAction.LeftButtonDown;
						break;
					case WM_LBUTTONUP:
						action = MouseAction.LeftButtonUp;
						break;
					case WM_RBUTTONDOWN:
						action = MouseAction.RightButtonDown;
						break;
					case WM_RBUTTONUP:
						action = MouseAction.RightButtonUp;
						break;
					case WM_MBUTTONDOWN:
						action = MouseAction.MiddleButtonDown;
						break;
					case WM_MBUTTONUP:
						action = MouseAction.MiddleButtonUp;
						break;
					case WM_MOUSEWHEEL:
						// Extract wheelDelta from high-order word of lParam
						int wheelDelta = ((int)Marshal.ReadInt32(lParam) >> 16) & 0xFFFF;
						wheelDelta = (short)wheelDelta; // Convert to signed short
						action = wheelDelta > 0 ? MouseAction.ScrollUp : MouseAction.ScrollDown;
						//System.Diagnostics.Debug.WriteLine($"WM_MOUSEWHEEL detected. nCode: {nCode} wParam: {wParam}, lParam: {lParam}, lParam64: {lParam.ToInt64()}, Extracted WheelDelta: {wheelDelta}, action: {action}");
						break;

					case WM_MOUSEMOVE: // Handle mouse move
						action = MouseAction.MouseMove;
						break;
					default:
						//System.Diagnostics.Debug.WriteLine($"unknown mouse nCode: {nCode} wParam: {wParam}, lParam: {lParam}");
						break;
				}

				if (action.HasValue)
				{
					MouseEvent?.Invoke(action.Value);
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

	public enum MouseAction
	{
		LeftButtonDown,
		LeftButtonUp,
		RightButtonDown,
		RightButtonUp,
		MiddleButtonDown,
		MiddleButtonUp,
		ScrollUp,
		ScrollDown,
		MouseMove 
	}

}
