using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MerTechStreamOverlay
{
	internal static class Program
	{
		private static Mutex mutex = null;

		// Import necessary functions from user32.dll
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_RESTORE = 9;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			const string appName = "MerTechStreamOverlay";
			bool createdNew;

			// Create a mutex with a unique name for the application
			mutex = new Mutex(true, appName, out createdNew);

			if (!createdNew)
			{
				// Bring the existing instance to the foreground
				BringToForeground(appName);
				return; // Exit the application
			}

			try
			{
				ApplicationConfiguration.Initialize();
				Application.Run(new FormMain());
			}
			finally
			{
				if (mutex != null)
				{
					mutex.ReleaseMutex();
				}
			}
		}

		private static void BringToForeground(string windowTitle)
		{
			IntPtr hWnd = FindWindow(null, windowTitle);
			if (hWnd != IntPtr.Zero)
			{
				if (IsIconic(hWnd)) // If minimized, restore it
				{
					ShowWindow(hWnd, SW_RESTORE);
				}
				SetForegroundWindow(hWnd); // Bring to the foreground
			}
		}

	}
}
