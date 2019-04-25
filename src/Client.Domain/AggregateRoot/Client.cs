using System;
using System.Collections.Generic;
using Client.Domain.AggregateRoot.ClientAggregateRoot;
using MSFramework.Domain;

namespace Client.Domain.AggregateRoot
{
	public class Client : AggregateRootBase
	{
		private bool _isDeleted;

		/// <summary>
		/// 客户名称
		/// </summary>
		private string Name;

		/// <summary>
		/// 客户简称
		/// </summary>
		private string ShortName;

		/// <summary>
		/// 客户类型 -> 公墓基金、私募基金
		/// </summary>
		private string Type;

		/// <summary>
		/// 城市
		/// </summary>
		private string City;

		/// <summary>
		/// 省
		/// </summary>
		private string Province;

		/// <summary>
		/// 国家
		/// </summary>
		private string Country;

		/// <summary>
		/// 优先级
		/// </summary>
		private string Level;

		/// <summary>
		/// 支付方式
		/// </summary>
		private string PaymentType;

		/// <summary>
		/// 打分周期
		/// </summary>
		private string ScoringCycle;

		/// <summary>
		/// 状态	销售线索、潜在客户、试用客户、活跃客户
		/// </summary>
		private string State;

		/// <summary>
		/// 启用/禁用
		/// </summary>
		private bool Active;

		public ClientUser ClientUser { get; private set; }


		public Client(
			string name, string shortName, string type, string city, string province,
			string country, string level, string paymentType, string scoringCycle, string state, bool active,
			List<ClientUser> clientUsers
		)
		{
			ApplyAggregateEvent(new ClientCreatedEvent(name, shortName, type, city, province,
				country, level, paymentType, scoringCycle, state, active,
				clientUsers));
		}

		private void Apply(ClientCreatedEvent e)
		{
			Version = e.Version;
			Id = e.AggregateId;
			Name = e.Name;
			ShortName = e.ShortName;
			Type = e.Type;
			City = e.City;
			Province = e.Province;
			Country = e.Country;
			Level = e.Level;
			PaymentType = e.PaymentType;
			ScoringCycle = e.ScoringCycle;
		}

		private void Apply(ClientChangedEvent e)
		{
			Version = e.Version;
			ClientUser = e.NewClientUser;
		}

		private void Apply(ClientDeletedEvent e)
		{
			Version = e.Version;
			_isDeleted = true;
		}

		public void ChangeClientUser(ClientUser newClientUser)
		{
			if (newClientUser == null)
			{
				throw new ArgumentException(nameof(newClientUser));
			}

			ApplyAggregateEvent(new ClientChangedEvent(newClientUser));
		}

		public void Delete()
		{
			ApplyAggregateEvent(new ClientDeletedEvent());
		}

		/// <summary>
		/// 修改客户基本信息
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="clientDto"></param>
		public void ChangeBasicInfo(Guid guid, Client client)
		{
		}

		/// <summary>
		/// Enable客户
		/// </summary>
		/// <param name="Id"></param>
		public void EnableClient(Guid Id)
		{
		}

		/// <summary>
		/// disable客户
		/// </summary>
		/// <param name="Id"></param>
		public void DisableClient(Guid Id)
		{
		}

		/// <summary>
		/// 软删除客户
		/// </summary>
		/// <param name="Id"></param>
		public void SoftDeleteClient(Guid Id)
		{
		}

		//客户联系人部分
		/// <summary>
		/// 添加客户联系人
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="clientUser"></param>
		public void AddClientUser(Guid clientId, ClientUser clientUser)
		{
		}

		/// <summary>
		/// 修改客户联系人信息
		/// </summary>
		/// <param name="clientUserId"></param>
		/// <param name="clientUser"></param>
		public void ChangeClientUser(Guid clientUserId, ClientUser clientUser)
		{
		}

		/// <summary>
		/// 软删除客户联系人
		/// </summary>
		/// <param name="clientUserId"></param>
		public void SoftDeleteClientUser(Guid clientUserId)
		{
		}

		/// <summary>
		/// 关联客户账单
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="invoiceId"></param>
		public void AssignInvoice(Guid clientId, Guid invoiceId)
		{
		}

		/// <summary>
		/// 取消关联客户账单
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="invoiceId"></param>
		public void UnAssignInvoice(Guid clientId, Guid invoiceId)
		{
		}
	}
}