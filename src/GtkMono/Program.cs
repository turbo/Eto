using System;
using Eto.Test;

namespace GtkMono
{
	internal class Startup
	{
		[STAThread]
		static void Main(string[] args)
		{
			var generator = new Eto.GtkSharp.Platform();
			
			var app = new TestApplication(generator);
			app.TestAssemblies.Add(typeof(Startup).Assembly);
			app.Run();
		}
	}
}