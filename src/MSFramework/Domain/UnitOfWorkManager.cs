using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain
{
	/// <summary>
	/// 工作单元管理器
	/// </summary>
	public class UnitOfWorkManager
	{
		/// <summary>
		/// 工作单元集合
		/// </summary>
		private readonly HashSet<IUnitOfWork> _unitOfWorks;

		/// <summary>
		/// 初始化工作单元管理器
		/// </summary>
		public UnitOfWorkManager()
		{
			_unitOfWorks = new HashSet<IUnitOfWork>();
		}

		/// <summary>
		/// 提交
		/// </summary>
		public async Task CommitAsync()
		{
			foreach (var unitOfWork in _unitOfWorks)
			{
				await unitOfWork.CommitAsync();
			}
		}

		/// <summary>
		/// 注册工作单元
		/// </summary>
		/// <param name="unitOfWork">工作单元</param>
		public void Register(IUnitOfWork unitOfWork)
		{
			if (unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			_unitOfWorks.Add(unitOfWork);
		}

		public IReadOnlyCollection<IUnitOfWork> GetUnitOfWorks()
		{
			return _unitOfWorks;
		}
	}
}