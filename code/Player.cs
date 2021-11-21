namespace TerryForm
{
	public class Player : Sandbox.Player
	{
		public string lintTest { get; set; }

		public void Test() {
			/*
			 * This is deliberately misformatted to see if the linter actually
			 * catches it
			 */

			bool TestBool = false;
			Log.Trace( TestBool );
		}
	}
}

