using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThatDeveloperDad.iFX.ServiceModel.Taxonomy
{
	/// <summary>
	/// Identifies a component within the system as a member of the
	/// Engine Service Archetype.
	/// 
	/// An Engine service is a component that does "stuff".  
	/// 
	/// Calculations, transformations, decision making based on input data, etc...  
	/// Probably the most ubiquitous kind of Engine component is a Validation Engine, 
	/// if you're in the habit of pulling that code into its own class or library 
	/// within your systems.
	/// 
	/// Engine Services are only called by Manager Services.
	/// Engine Services MAY call into Resource Access Services.
	/// Engine Services MAY NOT call into other Engine Services.
	/// Engines MAY NOT call Manager Services.
	/// </summary>
	public interface IEngineService : ISystemComponent
	{
	}
}
