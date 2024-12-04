using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThatDeveloperDad.iFX.ServiceModel.Taxonomy
{
	/// <summary>
	/// Identifies a service within the system that acts as 
	/// a Public Client to the subsystems materialized within.
	/// 
	/// In a solution that does not include a UserInterface client, there must at least be
	/// some kind of API Client that provides the public "face" of this system.
	/// 
	/// Client Services are the ultimate Top of a System Architecture, and may only
	/// call into Manager Services.
	/// </summary>
	public interface IClientService : ISystemComponent
	{
	}
}
