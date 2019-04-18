using System;
using System.Collections.Generic;
using System.Text;

namespace MSFramework.Domain.Auditing
{
	/// <summary>
	/// This interface is implemented by entities that is wanted to store modification information (who and when modified lastly).
	/// Properties are automatically set when updating the <see cref="IEntity"/>.
	/// </summary>
	public interface IModificationAudited : IHasModificationTime
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string LastModifierUserId { get; set; }
	}
}
