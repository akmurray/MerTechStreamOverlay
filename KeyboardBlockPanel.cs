using System.Collections.Generic;
using System.Windows.Forms;

namespace MerTechStreamOverlay
{
	public class KeyboardBlockPanel : BlockPanel
	{
		private GlobalKeyboardHook keyboardHook;

		// Mapping of keys to positions in a 21x6 grid
		private readonly Dictionary<Keys, (int col, int row, int width)> keyMappings = new Dictionary<Keys, (int col, int row, int width)>
		{
            // Row 0: Top Function Keys
            { Keys.Escape, (0, 0, 1) },
			{ Keys.F1, (1, 0, 1) }, { Keys.F2, (2, 0, 1) }, { Keys.F3, (3, 0, 1) }, { Keys.F4, (4, 0, 1) },
			{ Keys.F5, (6, 0, 1) }, { Keys.F6, (7, 0, 1) }, { Keys.F7, (8, 0, 1) }, { Keys.F8, (9, 0, 1) },
			{ Keys.F9, (10, 0, 1) }, { Keys.F10, (11, 0, 1) }, { Keys.F11, (12, 0, 1) }, { Keys.F12, (13, 0, 1) }, 
			{ Keys.PrintScreen, (14, 0, 1) },

            // Row 1: Number row
			{ Keys.Oemtilde, (0, 1, 1) }, { Keys.D1, (1, 1, 1) }, { Keys.D2, (2, 1, 1) }, { Keys.D3, (3, 1, 1) }, { Keys.D4, (4, 1, 1) },
			{ Keys.D5, (5, 1, 1) }, { Keys.D6, (6, 1, 1) }, { Keys.D7, (7, 1, 1) }, { Keys.D8, (8, 1, 1) },
			{ Keys.D9, (9, 1, 1) }, { Keys.D0, (10, 1, 1) }, { Keys.OemMinus, (11, 1, 1) }, { Keys.Oemplus, (12, 1, 1) },
			{ Keys.Back, (13, 1, 2) }, // Backspace spans 2 blocks

			// Additional keys after Backspace
			{ Keys.Insert, (15, 1, 1) }, // Insert key
			{ Keys.Home, (16, 1, 1) },   // Home key
			{ Keys.PageUp, (17, 1, 1) },  // Page Up key


            // Row 2: QWERTY row
			{ Keys.Tab, (0, 2, 2) }, // Tab spans 2 blocks
			{ Keys.Q, (2, 2, 1) }, { Keys.W, (3, 2, 1) }, { Keys.E, (4, 2, 1) },
			{ Keys.R, (5, 2, 1) }, { Keys.T, (6, 2, 1) }, { Keys.Y, (7, 2, 1) }, { Keys.U, (8, 2, 1) },
			{ Keys.I, (9, 2, 1) }, { Keys.O, (10, 2, 1) }, { Keys.P, (11, 2, 1) }, { Keys.OemOpenBrackets, (12, 2, 1) },
			{ Keys.OemCloseBrackets, (13, 2, 1) }, { Keys.OemBackslash, (14, 2, 1) },

			// Additional keys after OemBackslash
			{ Keys.Delete, (15, 2, 1) }, // Delete key
			{ Keys.End, (16, 2, 1) },    // End key
			{ Keys.PageDown, (17, 2, 1) }, // Page Down key

			// Numpad keys
			{ Keys.NumPad7, (18, 2, 1) }, // Numpad 7
			{ Keys.NumPad8, (19, 2, 1) }, // Numpad 8
			{ Keys.NumPad9, (20, 2, 1) },  // Numpad 9


			// Row 3: ASDF row
			{ Keys.CapsLock, (0, 3, 2) }, // CapsLock spans 2 blocks
			{ Keys.A, (2, 3, 1) }, { Keys.S, (3, 3, 1) }, { Keys.D, (4, 3, 1) },
			{ Keys.F, (5, 3, 1) }, { Keys.G, (6, 3, 1) }, { Keys.H, (7, 3, 1) }, { Keys.J, (8, 3, 1) },
			{ Keys.K, (9, 3, 1) }, { Keys.L, (10, 3, 1) }, { Keys.OemSemicolon, (11, 3, 1) }, { Keys.OemQuotes, (12, 3, 1) },
			{ Keys.Enter, (13, 3, 2) }, // Enter spans 2 blocks

			// Skip 3 columns after Enter
			{ Keys.NumPad4, (17, 3, 1) }, // Numpad 4
			{ Keys.NumPad5, (18, 3, 1) }, // Numpad 5
			{ Keys.NumPad6, (19, 3, 1) }, // Numpad 6
			{ Keys.Add, (20, 3, 1) },      // Plus (+)


			// Row 4: ZXCV row and additional keys
			{ Keys.LShiftKey, (0, 4, 2) }, // Left Shift spans 2 blocks
			{ Keys.Z, (2, 4, 1) }, { Keys.X, (3, 4, 1) }, { Keys.C, (4, 4, 1) },
			{ Keys.V, (5, 4, 1) }, { Keys.B, (6, 4, 1) }, { Keys.N, (7, 4, 1) }, { Keys.M, (8, 4, 1) },
			{ Keys.Oemcomma, (9, 4, 1) }, { Keys.OemPeriod, (10, 4, 1) }, { Keys.OemQuestion, (11, 4, 1) },
			{ Keys.RShiftKey, (13, 4, 2) }, // Right Shift spans 2 blocks
			{ Keys.Up, (16, 4, 1) }, // Up Arrow key, skips space after Right Shift
			// Space before Numpad 1, 2, 3
			{ Keys.NumPad1, (18, 4, 1) }, { Keys.NumPad2, (19, 4, 1) }, { Keys.NumPad3, (20, 4, 1) },

            /// Row 5: Spacebar and Arrow/Numpad row
			{ Keys.LControlKey, (0, 5, 1) },
			{ Keys.LWin, (1, 5, 1) },
			{ Keys.LMenu, (2, 5, 1) }, // Fixed position for LMenu
			{ Keys.Space, (3, 5, 5) }, // Space spans 5 blocks
			{ Keys.RMenu, (8, 5, 1) },
			{ Keys.RControlKey, (9, 5, 1) },

			// Arrow keys
			{ Keys.Left, (10, 5, 1) },
			{ Keys.Down, (11, 5, 1) },
			{ Keys.Right, (12, 5, 1) },

			// Numpad keys
			{ Keys.NumPad0, (13, 5, 2) }, // 0 spans 2 blocks
			{ Keys.Decimal, (15, 5, 1) }, // Numpad period
			//{ Keys.NumEnter, (16, 5, 1) } // Numpad enter

		};

		public KeyboardBlockPanel(int blockSize = 20, int blocksWide = 21, int blocksTall = 6)
			: base(blockSize, blocksWide, blocksTall)
		{
			InitializeKeyboardHook();
		}

		private void InitializeKeyboardHook()
		{
			keyboardHook = new GlobalKeyboardHook();
			keyboardHook.KeyActions += OnKeyAction;
		}

		private void OnKeyAction(KeyAction action, Keys key)
		{
			if (keyMappings.TryGetValue(key, out var position))
			{
				int blockIndex = position.row * BlocksWide + position.col;
				//Console.WriteLine($"OnKeyAction({action}, {key}) Activating at row {position.row}, col {position.col}, width {position.width} ... blockIndex: {blockIndex}");

				if (action == KeyAction.KeyDown)
				{
					ActivateBlock(blockIndex, position.width);
				}
				else if (action == KeyAction.KeyUp)
				{
					DeactivateBlock(blockIndex, position.width);
				}
			}
			else
			{
				//Console.WriteLine($"OnKeyAction({action}, {key}) MISS");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && keyboardHook != null)
			{
				keyboardHook.Dispose();
				keyboardHook = null;
			}
			base.Dispose(disposing);
		}
		public void Pause()
		{
			//timer?.Stop();
		}

		public void Resume()
		{
			//timer?.Stop();
		}
	}
}
