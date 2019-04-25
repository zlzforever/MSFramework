using System;
using MSFramework.Common;

namespace MSFramework.Data
{
	public interface IIdGenerator
	{
		T GetNewId<T>();
	}

	public class IdGenerator : IIdGenerator
	{
		private readonly Type _guidType = typeof(Guid);

		public T GetNewId<T>()
		{
			var type = typeof(T);
			if (type == _guidType)
			{
				return (T) GetNewGuid();
			}
			else
			{
				return default;
			}
		}

		protected virtual dynamic GetNewGuid()
		{
			return CombGuid.NewGuid();
		}
	}
}