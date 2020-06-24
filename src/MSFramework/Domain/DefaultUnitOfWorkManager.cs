using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Audit;

namespace MSFramework.Domain
{
	/// <summary>
	/// 工作单元管理器
	/// </summary>
	public class DefaultUnitOfWorkManager : IUnitOfWorkManager
	{
		/// <summary>
		/// 工作单元集合
		/// </summary>
		private readonly List<IUnitOfWork> _unitOfWorks;

		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// 初始化工作单元管理器
		/// </summary>
		public DefaultUnitOfWorkManager(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_unitOfWorks = new List<IUnitOfWork>();
		}

		/// <summary>
		/// 提交
		/// </summary>
		public void Commit()
		{
			// 1. commit 完成则所有实体的变化信息被清空，无法还原
			// 2. 审计信息通过审核服务来处理，即可以直接存储，也可以通过消息队列推出去。
			var auditService = _serviceProvider.GetService<IAuditService>();
			if (auditService == null)
			{
				foreach (var unitOfWork in _unitOfWorks)
				{
					unitOfWork.Commit();
				}
			}
			else
			{
				foreach (var unitOfWork in _unitOfWorks)
				{
					auditEntities.AddRange(unitOfWork.GetAuditEntries());
					unitOfWork.Commit();
				}
				auditService.SaveAsync()
			}
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

			if (_unitOfWorks.Contains(unitOfWork) == false)
			{
				_unitOfWorks.Add(unitOfWork);
			}
		}
	}
}