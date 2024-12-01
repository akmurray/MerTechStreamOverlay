namespace MerTechStreamOverlay
{
	public class MouseMessageFilter : IMessageFilter
	{
		private Action<MouseButtons, bool> mouseEventHandler;

		public MouseMessageFilter(Action<MouseButtons, bool> handler)
		{
			mouseEventHandler = handler;
		}

		public bool PreFilterMessage(ref Message m)
		{
			const int WM_LBUTTONDOWN = 0x0201;
			const int WM_LBUTTONUP = 0x0202;
			const int WM_MBUTTONDOWN = 0x0207;
			const int WM_MBUTTONUP = 0x0208;
			const int WM_RBUTTONDOWN = 0x0204;
			const int WM_RBUTTONUP = 0x0205;

			if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONUP)
			{
				mouseEventHandler(MouseButtons.Left, m.Msg == WM_LBUTTONDOWN);
				return false;
			}
			if (m.Msg == WM_MBUTTONDOWN || m.Msg == WM_MBUTTONUP)
			{
				mouseEventHandler(MouseButtons.Middle, m.Msg == WM_MBUTTONDOWN);
				return false;
			}
			if (m.Msg == WM_RBUTTONDOWN || m.Msg == WM_RBUTTONUP)
			{
				mouseEventHandler(MouseButtons.Right, m.Msg == WM_RBUTTONDOWN);
				return false;
			}

			return false;
		}

	}
}
