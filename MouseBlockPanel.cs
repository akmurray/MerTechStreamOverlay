using System.Windows.Forms;

namespace MerTechStreamOverlay
{
	public class MouseBlockPanel : BlockPanel
	{
		private GlobalMouseHook mouseHook;
		private System.Windows.Forms.Timer scrollTimer; // Timer to reset scrolling blocks

		public MouseBlockPanel(int blockSize = 20, int blocksWide = 4, int blocksTall = 1)
			: base(blockSize, blocksWide, blocksTall)
		{
			InitializeMouseHook();

			// Initialize scroll timer
			scrollTimer = new System.Windows.Forms.Timer { Interval = 100 }; // 100 ms delay
			scrollTimer.Tick += (s, e) =>
			{
				// Clear scroll blocks when the timer ticks
				DeactivateBlock(3); // Deactivate scroll up/down block
				scrollTimer.Stop();
			};
		}

		private void InitializeMouseHook()
		{
			mouseHook = new GlobalMouseHook();
			mouseHook.MouseEvent += OnMouseAction;
		}

		private void OnMouseAction(MouseAction action)
		{
			switch (action)
			{
				case MouseAction.LeftButtonDown:
					ActivateBlock(0); // Left click
					break;
				case MouseAction.LeftButtonUp:
					DeactivateBlock(0);
					break;
				case MouseAction.MiddleButtonDown:
					ActivateBlock(2); // Middle click
					break;
				case MouseAction.MiddleButtonUp:
					DeactivateBlock(2);
					break;
				case MouseAction.RightButtonDown:
					ActivateBlock(1); // Right click
					break;
				case MouseAction.RightButtonUp:
					DeactivateBlock(1);
					break;
				case MouseAction.ScrollUp:
				case MouseAction.ScrollDown:
					HandleScrollBlock(); // Activate scroll block
					break;
				case MouseAction.MouseMove:
					HighlightBorder();
					break;
			}
		}

		private void HandleScrollBlock()
		{
			ActivateBlock(3); // Activate scroll block
			var timer = new System.Windows.Forms.Timer { Interval = 100 }; // Reset after 100ms
			timer.Tick += (s, e) =>
			{
				DeactivateBlock(3);
				timer.Dispose();
			};
			timer.Start();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && mouseHook != null)
			{
				mouseHook.Dispose();
				mouseHook = null;
			}
			base.Dispose(disposing);
		}
		public void Pause()
		{
			scrollTimer?.Stop();
		}

		public void Resume()
		{
			scrollTimer?.Start();
		}

	}
}
