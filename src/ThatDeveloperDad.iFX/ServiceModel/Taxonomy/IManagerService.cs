using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThatDeveloperDad.iFX.ServiceModel.Taxonomy
{
	/// <summary>
	/// Identifies the component that includes this interface as being 
	/// a member of the Manager service archetype.
	/// 
	/// Manager Services use Engine and ResourceAccess Services to perform
	/// specific Use Cases.  (Like an Orchestror or Conductor, of sorts.)
	/// 
	/// Methods exposed by Manager Services must be considered the exclusive 
	/// entrypoint to the capabilities defined by the Engine and Resouce Services;
	/// Those Service kinds must never be called directly from outside the system.
	/// 
	/// Managers may call Engines and ResourceAccessors.
	/// Managers may not directly call other Manager Services.
	/// </summary>
	public interface IManagerService : ISystemComponent 
	{ 
		
	}
}
