using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using ThatDeveloperDad.iFX.ServiceModel;

namespace ThatDeveloperDad.iFX.ServiceModel.Taxonomy
{
	/// <summary>
	/// Identifies a component with this marker interface as a Formal 
	/// System Service within the SystemModel Taxonomy.
	/// </summary>
	public interface ISystemComponent
	{
		internal IEnumerable<Type> ServiceArchetypes
		{
			get{
				var archetypes = new List<Type>(){
					typeof(IManagerService),
					typeof(IUtilityService),
					typeof(IEngineService),
					typeof(IResourceAccessService),
					typeof(IClientService)
				};
				return archetypes;
			}
		}
	}
}
