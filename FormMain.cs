using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace MerTechStreamOverlay
{
	public partial class FormMain : Form
	{
		private NotifyIcon trayIcon;
		private ContextMenuStrip trayMenu;

		// Variables to track dragging
		private bool dragging = false;
		private Point dragCursorPoint;
		private Point dragFormPoint;

		private MouseBlockPanel mousePanel;
		private KeyboardBlockPanel keyboardPanel;
		private CpuBlockPanel cpuPanel;
		private int FormBorderWidth = 5;

		public static Color BlueColor = Color.FromArgb(0x74, 0xFC, 0xF8);
		public static Color OrangeColor = Color.FromArgb(0xFC, 0x89, 0x74);

		public FormMain()
		{
			InitializeComponent();

			// Set form icon
			this.Icon = new Icon("logo-tiny-32.ico");

			// Set up the form
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(304, 101);
			this.BackColor = Color.Black;
			this.TransparencyKey = this.BackColor;

			// Read opacity from app.config
			double opacity;
			if (double.TryParse(ConfigurationManager.AppSettings["Opacity"], out opacity))
			{
				this.Opacity = opacity; // Set the form's opacity based on the configuration
			}
			else
			{
				// Handle the case where parsing fails, maybe set a default opacity
				this.Opacity = 0.9; // Default value from your config
			}

			// Set up mouse events for dragging
			this.MouseDown += FormMain_MouseDown;
			this.MouseMove += FormMain_MouseMove;
			this.MouseUp += FormMain_MouseUp;

			// Set up the system tray icon
			trayMenu = new ContextMenuStrip();
			trayMenu.Items.Add("Show", null, OnShowClicked);
			trayMenu.Items.Add("Exit", null, OnExitClicked);

			trayIcon = new NotifyIcon
			{
				Icon = this.Icon,
				ContextMenuStrip = trayMenu,
				Visible = true,
				Text = "MerTech Stream Overlay" // Tooltip for the tray icon
			};

			trayIcon.MouseClick += TrayIcon_MouseClick;

			var CpuPanelHeight = 15;
			AddKeyboardAndMousePanels(FormBorderWidth, CpuPanelHeight + FormBorderWidth);
			AddCpuPanel();
			AddLogoToForm();
		}

		
		private void AddKeyboardAndMousePanels(int xOffset, int yOffset)
		{
			keyboardPanel = new KeyboardBlockPanel(12, 21, 6)
			{
				BlockColor = Color.DimGray,
				ActiveBlockColor = OrangeColor,
				BorderColor = Color.Black,
				Location = new Point(xOffset, yOffset) 
			};
			this.Controls.Add(keyboardPanel);

			// Add the mouse visualization panel
			mousePanel = new MouseBlockPanel(14, 2, 2)
			{
				BlockColor = Color.DimGray,
				ActiveBlockColor = OrangeColor,
				BorderColor = Color.Black,
				Location = new Point(1 + xOffset/2 + (keyboardPanel.BlockSize * keyboardPanel.BlocksWide) + (FormBorderWidth * 2), yOffset)
			};
			this.Controls.Add(mousePanel);
		}
		private void AddCpuPanel()
		{
			// Calculate the bottom position of the form
			int panelHeight = 10; // Block size for height
			int formHeight = this.ClientSize.Height;

			// Create CPU panel
			cpuPanel = new CpuBlockPanel(10, 29, 1)
			{
				BlockColor = Color.DimGray,
				ActiveBlockColor = OrangeColor,
				BorderColor = Color.Transparent,
				Location = new Point(FormBorderWidth, FormBorderWidth) // Positioned at the top
				//Location = new Point(3, formHeight - panelHeight - FormBorderWidth) // Positioned at the bottom
			};

			this.Controls.Add(cpuPanel);
		}

		private void FormMain_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				dragging = true;
				dragCursorPoint = Cursor.Position;
				dragFormPoint = this.Location;
			}
		}

		private void FormMain_MouseMove(object sender, MouseEventArgs e)
		{
			if (dragging)
			{
				Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
				this.Location = Point.Add(dragFormPoint, new Size(diff));
			}
		}

		private void FormMain_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				dragging = false;
			}
		}

		private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.Visible)
				{
					// If the form is visible, hide it
					this.Hide();
				}
				else
				{
					// If the form is hidden, show and bring it to the foreground
					this.Show();
					this.WindowState = FormWindowState.Normal; // Restore if minimized
					this.BringToFront(); // Bring to the foreground
					this.Activate(); // Ensure the form gets focus
				}
			}
		}

		private void OnShowClicked(object sender, EventArgs e)
		{
			// Show the form and bring it to the front
			this.Show();
			this.BringToFront();
		}

		private void OnExitClicked(object sender, EventArgs e)
		{
			// Cleanup and exit the application
			trayIcon.Dispose();
			//doesn't kill the process. only the form and tray icon: Application.Exit();
			Environment.Exit(0);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			// Prevent the form from closing and just hide it instead
			e.Cancel = true;
			this.Hide();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// Draw a border
			using (Pen pen = new Pen(BlueColor, 5)) // 5px thick border
			{
				// Adjust the rectangle to avoid clipping
				Rectangle borderRectangle = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
				e.Graphics.DrawRectangle(pen, borderRectangle);
			}
		}
		private void AddLogoToForm()
		{
			try
			{
				// Load the image from the root of the project
				Image logoImage = Image.FromFile("logo-tiny-32.png");

				// Create a PictureBox to display the image
				PictureBox logoPictureBox = new PictureBox
				{
					Image = new Bitmap(logoImage, new Size(32, 37)), // Resize to 32x37
					SizeMode = PictureBoxSizeMode.StretchImage,
					Size = new Size(32, 37), // Set the size of the PictureBox
					BackColor = Color.Transparent
				};

				// Position the PictureBox in the lower-right corner of the form
				int x = this.ClientSize.Width - logoPictureBox.Width - FormBorderWidth - 2;
				int y = this.ClientSize.Height - logoPictureBox.Height - FormBorderWidth - 2;
				logoPictureBox.Location = new Point(x, y);

				// Add the PictureBox to the form's controls
				this.Controls.Add(logoPictureBox);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading logo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			bool disableWhenNotVisible = false;
			if (disableWhenNotVisible)
			{
				if (this.Visible)
				{
					// Resume all panels' timers
					cpuPanel?.Resume();
					mousePanel?.Resume();
					keyboardPanel?.Resume();
				}
				else
				{
					// Pause all panels' timers
					cpuPanel?.Pause();
					mousePanel?.Pause();
					keyboardPanel?.Pause();
				}
			}
		}

	}
}
