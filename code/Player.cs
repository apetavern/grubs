namespace TerryForm
{
	public class Player : Sandbox.Player
	{
		public string LintTest { get; set; }

		public void Test()
		{
			/*
			 * This is deliberately misformatted to see if the linter actually
			 * catches it
			 */

			bool testBool = false;
			Log.Trace( testBool );
		}
	}
}

