using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;

namespace AvaloniaCrossTest.Services
{
	public interface IMainWindowProvider
	{
		Window MainWindow { get; set; }
	}
}
