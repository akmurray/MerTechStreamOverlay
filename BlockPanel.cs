using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MerTechStreamOverlay
{
	public class BlockPanel : Panel
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BlockSize { get; set; } = 5; // Default block size (5x5 pixels)

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BlocksWide { get; private set; } // Number of blocks horizontally

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BlocksTall { get; private set; } // Number of blocks vertically

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color BlockColor { get; set; } = Color.Gray; // Default block color

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BorderWidth { get; set; } = 1; // Default border width

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color BorderColor { get; set; } = Color.Black; // Border color

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color HighlightColor { get; set; } = Color.Yellow; // Border highlight color

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color HighlightBackgroundColor { get; set; } = FormMain.OrangeColor; // Background color when highlighted

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color DefaultBackgroundColor { get; set; } = Color.DarkGray; // Default background color

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color ActiveBlockColor { get; set; } = Color.Green; // Color for active blocks

		private bool[] blockStates;

		private bool isHighlighted = false;
		private System.Windows.Forms.Timer highlightTimer;

		// Default constructor
		public BlockPanel() : this(5, 10, 10) { } // Default grid of 10x10 blocks

		// Constructor with custom width and height in blocks
		public BlockPanel(int blockSize, int blocksWide, int blocksTall)
		{
			this.DoubleBuffered = true; // Reduce flickering
			this.BlockSize = blockSize;

			// Account for border width in panel dimensions
			this.Width = blocksWide * BlockSize + 2 * BorderWidth + 2;
			this.Height = blocksTall * BlockSize + 2 * BorderWidth + 2;
			this.BlocksWide = blocksWide;
			this.BlocksTall = blocksTall;

			this.BorderStyle = BorderStyle.FixedSingle;

			highlightTimer = new System.Windows.Forms.Timer();
			highlightTimer.Interval = 61; // Highlight duration in milliseconds
			highlightTimer.Tick += (s, e) =>
			{
				isHighlighted = false;
				this.BackColor = DefaultBackgroundColor; // Reset background color
				highlightTimer.Stop();
				this.Invalidate(); // Redraw the panel
			};

			// Initialize block states
			blockStates = new bool[blocksWide * blocksTall];
			this.BackColor = DefaultBackgroundColor; // Set default background color
		}

		public void HighlightBorder()
		{
			isHighlighted = true;
			this.BackColor = HighlightBackgroundColor; // Change background color
			highlightTimer.Stop(); // Restart the timer
			highlightTimer.Start();
			this.Invalidate(); // Redraw the panel
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			int cols = BlocksWide;
			int rows = BlocksTall;

			// Adjust the starting position to consider the panel border
			int startX = BorderWidth;
			int startY = BorderWidth;

			// Effective block size after considering cell borders
			int effectiveBlockSize = BlockSize - 1; // Adjust for a 1-pixel border around each block

			// Draw the blocks
			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					// Calculate block position, adjusted for panel border
					int x = startX + col * BlockSize;
					int y = startY + row * BlockSize;

					// Determine the index and state
					int index = row * BlocksWide + col;
					bool isActive = index < blockStates.Length && blockStates[index];

					// Set the color
					using (Brush blockBrush = new SolidBrush(isActive ? ActiveBlockColor : BlockColor))
					{
						// Draw block within the effective size
						e.Graphics.FillRectangle(blockBrush, x, y, effectiveBlockSize, effectiveBlockSize);
					}

					// Draw border for each block
					using (Pen borderPen = new Pen(BorderColor, 1))
					{
						e.Graphics.DrawRectangle(borderPen, x, y, effectiveBlockSize, effectiveBlockSize);
					}
				}
			}
			
			//// Draw panel border if highlighted
			//if (isHighlighted)
			//{
			//	using (Pen highlightPen = new Pen(HighlightColor, 3)) // Highlight border width
			//	{
			//		Rectangle borderRectangle = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
			//		e.Graphics.DrawRectangle(highlightPen, borderRectangle);
			//	}
			//}
		}

		public void ActivateBlock(int index, int width = 1)
		{
			for (int i = 0; i < width; i++)
			{
				int blockIndex = index + i;
				if (blockIndex >= 0 && blockIndex < blockStates.Length)
				{
					blockStates[blockIndex] = true;
				}
			}
			this.Invalidate(); // Redraw the panel
		}

		public void DeactivateBlock(int index, int width = 1)
		{
			for (int i = 0; i < width; i++)
			{
				int blockIndex = index + i;
				if (blockIndex >= 0 && blockIndex < blockStates.Length)
				{
					blockStates[blockIndex] = false;
				}
			}
			this.Invalidate(); // Redraw the panel
		}
	}
}
