using System;
using System.Collections.Generic;
using System.Reflection;
using MSFramework.Domain.Event;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 创建周度计划
	/// </summary>
	public class CreateWeekSchedulerEvent : AggregateEventBase
	{
		public User User { get; }

		public Product Product { get; }

		public DateTime BeginTime { get; }

		public DateTime EndTime { get; }

		public CreateWeekSchedulerEvent(User user, Product product, DateTime beginTime, DateTime endTime)
		{
			User = user;
			Product = product;
			BeginTime = beginTime;
			EndTime = endTime;
		}
	}

	/// <summary>
	/// 添加可路演时间
	/// </summary>
	public class AddIdleDateTimeEvent : AggregateEventBase
	{
		public Appointment NewDateTime { get; }

		public AddIdleDateTimeEvent(Appointment freeDateTime)
		{
			NewDateTime = freeDateTime;
		}
	}

	/// <summary>
	/// 预约路演事件
	/// </summary>
	public class MakeAppointWithClientEvent : AggregateEventBase
	{
		/// <summary>
		/// 预约项
		/// </summary>
		public Guid AppointmentId { get; }
		
		public string Address { get; }

		public Client Client { get; }

		public List<ClientUser> ClientUsers { get; }

		public Sale Sale { get; }

		/// <summary>
		/// 预定时间
		/// </summary>
		public DateTime BookTime { get; }

		public string Description { get; }

		public MakeAppointWithClientEvent(Guid id, string address, Client client, List<ClientUser> clientUsers, Sale sale, string description, DateTime bookTime)
		{
			Address = address;
			Client = client;
			ClientUsers = clientUsers;
			Sale = sale;
			AppointmentId = id;
			BookTime = bookTime;
			Description = description;
		}
	}

	/// <summary>
	/// 删除预约事件
	/// </summary>
	public class CancelAppointEvent : AggregateEventBase
	{
		public Guid AppointmentId { get; }

		public CancelAppointEvent(Guid appointmentId)
		{
			AppointmentId = appointmentId;
		}
	}

	/// <summary>
	/// 移除可路演时间
	/// </summary>
	public class RemoveIdleDateTimeEvent : AggregateEventBase
	{
		public Guid AppointmentId { get; }
		
		public RemoveIdleDateTimeEvent(Guid appointmentId)
		{
			AppointmentId = appointmentId;
		}
	}

	/// <summary>
	/// 关联核心观点和主题
	/// </summary>
	public class BindKeyIdeaAndTopicEvent : AggregateEventBase
	{
		public Product Product { get; }

		public string KeyIdeaAndTopic { get; }

		public BindKeyIdeaAndTopicEvent(Product product, string keyIdeaAndTopic)
		{
			Product = product;
			KeyIdeaAndTopic = keyIdeaAndTopic;
		}
	}
}