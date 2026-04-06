using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;

namespace AvaloniaCrossTest.Services
{
	public class MainWindowProvider : IMainWindowProvider
	{
		public Window MainWindow { get; set; }
	}
}
