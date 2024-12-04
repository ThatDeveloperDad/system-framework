using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThatDeveloperDad.iFX.ServiceModel.Taxonomy
{
	/// <summary>
	/// Resource Access Services encapsulate the technical details and
	/// volatilities inherent with Storing and Retrieving instances of
	/// our System's Subject Instances.
	/// 
	/// The Concept of RESOURCES is not limited to these System Subjects, however,
	/// and may also refer to capabilities or information provided by Remote
	/// systems that are not directly connected to the current System Scope.
	/// 
	/// Therefore, a RESOURCE could be:
	///  - A User Account in some trusted Identity Store (MSFT, GOOG, AAPL, META, etc...)
	///  - A Record in our system's relational or NoSql database.
	///  - A FIle or other Data Object stored in a StorageService (Cloud or otherwise)(
	///  - A Remote API that can perform parts of our Workflows for us.
	///  - Remote IoT Devices
	///  - or even an AI such as a Large Language Model or other MachineLearning service.
	///  
	/// Because of this wide variety of Resource KINDS, along with the need to
	/// adapt quicky to changes in other systems' APIs, we need to encapsulate 
	/// the complexity and volatility of accessing these resources into a stable
	/// interface for the rest of our System to consume.
	/// 
	/// ResourceAccessors may not call "Up" the stack.
	/// ResourceAccessors may be called by either Managers or Engines.
	/// ResourceAccessors may not call "across" to other ResourceAccessors.
	/// </summary>
	public interface IResourceAccessService : ISystemComponent
	{
	}
}
