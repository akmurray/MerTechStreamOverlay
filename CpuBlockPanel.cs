using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MerTechStreamOverlay
{
	public class CpuBlockPanel : BlockPanel
	{
		private System.Windows.Forms.Timer updateTimer;
		private PerformanceCounter cpuIdleCounter;

		public CpuBlockPanel(int blockSize = 3, int blocksWide = 100, int blocksTall = 1)
			: base(blockSize, blocksWide, blocksTall)
		{
			InitializeCpuCounter();
			InitializeUpdateTimer();
		}

		private void InitializeCpuCounter()
		{
			// Using % Idle Time to calculate CPU usage
			cpuIdleCounter = new PerformanceCounter("Processor", "% Idle Time", "_Total");
			cpuIdleCounter.NextValue(); // Warm up the counter
		}

		private void InitializeUpdateTimer()
		{
			updateTimer = new System.Windows.Forms.Timer { Interval = 500 }; // Update every 500ms
			updateTimer.Tick += UpdateCpuUsage;
			updateTimer.Start();
		}

		private void UpdateCpuUsage(object sender, EventArgs e)
		{
			try
			{
				// Calculate CPU usage as 100% - Idle Time
				float idleTime = cpuIdleCounter.NextValue();
				float cpuUsage = 100 - idleTime;

				// Map CPU usage to the number of active blocks
				int activeBlocks = (int)(cpuUsage / 100 * BlocksWide);

				// Clear all blocks and activate the appropriate number
				ClearBlocks();
				for (int i = 0; i < activeBlocks; i++)
				{
					ActivateBlock(i);
				}

				// Debugging output
				Debug.WriteLine($"[CPU Debug] Idle Time: {idleTime}% | CPU Usage: {cpuUsage}%");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error updating CPU usage: {ex.Message}");
			}
		}

		private void ClearBlocks()
		{
			for (int i = 0; i < BlocksWide; i++)
			{
				DeactivateBlock(i);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				updateTimer?.Stop();
				updateTimer?.Dispose();
				cpuIdleCounter?.Dispose();
			}
			base.Dispose(disposing);
		}
		public void Pause()
		{
			updateTimer?.Stop();
		}

		public void Resume()
		{
			updateTimer?.Start();
		}

	}
}
