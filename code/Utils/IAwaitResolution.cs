using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerryForm.Utils
{
	public interface IAwaitResolution
	{
		public bool IsResolved { get; set; }
	}
}
