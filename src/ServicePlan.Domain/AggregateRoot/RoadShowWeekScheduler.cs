using MSFramework.Domain;
using MSFramework.Domain.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSFramework.Data;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 路演周计划
	/// </summary>
	public class RoadShowWeekScheduler : AggregateRootBase
	{
		/// <summary>
		/// 路演
		/// </summary>
		private readonly List<Appointment> _appointments = new List<Appointment>(0);

		/// <summary>
		/// 研究员
		/// </summary>
		private User _user;

		/// <summary>
		/// 开始时间
		/// </summary>
		private DateTime _beginTime;

		/// <summary>
		/// 产品信息
		/// </summary>
		private Product _product;

		/// <summary>
		/// 结束时间
		/// </summary>
		private DateTime _endTime;

		/// <summary>
		/// 核心观点与服务
		/// </summary>
		private string _keyIdeaAndTopic;

		/// <summary>
		/// 可路演时间
		/// </summary>
		public IReadOnlyCollection<Appointment> Appointments => _appointments;

		/// <summary>
		/// 路演计划
		/// </summary>
		/// <param name="user">用户</param>
		/// <param name="beginTime">开始时间</param>
		/// <param name="endTime">结束时间</param>
		public RoadShowWeekScheduler(User user, DateTime beginTime, DateTime endTime)
		{
			ApplyAggregateEvent(new CreateWeekSchedulerEvent(user, beginTime, endTime));
		}

		/// <summary>
		/// 绑定路演关联的产品
		/// </summary>
		/// <param name="product">产品</param>
		public void BindProducts(Product product)
		{
			Check.NotNull(product, nameof(product));
			
			if (_product != null)
			{
				throw new ServicePlanException("已关联路演产品，无法更改");
			}
			
			ApplyAggregateEvent(new BindKeyIdeaAndTopicEvent(product));
		}

		/// <summary>
		/// 添加可路演时间
		/// </summary>
		/// <param name="location">可路演地点</param>
		/// <param name="beginTime">开始时间</param>
		/// <param name="endTime">结束时间</param>
		public void AddAppointment(string location, DateTime beginTime, DateTime endTime)
		{
			if (_product == null)
			{
				throw new ServicePlanException("未设置路演产品");
			}

			ApplyAggregateEvent(new AddIdleDateTimeEvent(new Appointment(location, beginTime, endTime)));
		}

		/// <summary>
		/// 删除可路演时间
		/// </summary>
		/// <param name="appointmentId">预约标识</param>
		public void DeleteAppointment(Guid appointmentId)
		{
			ApplyAggregateEvent(new RemoveIdleDateTimeEvent(appointmentId));
		}

		/// <summary>
		/// 销售预约客户路演
		/// </summary>
		/// <param name="appointmentId">预约id</param>
		/// <param name="address">路演地址</param>
		/// <param name="client">客户信息</param>
		/// <param name="clientUsers">客户联系人</param>
		/// <param name="sale">销售信息</param>
		/// <param name="description">描述</param>
		public void MakeAppointWithClient(Guid appointmentId, string address, Client client,
			List<ClientUser> clientUsers, User sale, string description)
		{
			Check.NotNull(address, nameof(address));
			Check.NotNull(client, nameof(client));
			Check.NotNull(clientUsers, nameof(clientUsers));
			Check.NotNull(sale, nameof(sale));

			ApplyAggregateEvent(new MakeAppointWithClientEvent(appointmentId, address, client, clientUsers, sale,
				description, DateTime.Now));

			// 发送添加服务计划的事件
			RegisterDomainEvent(
				new CreateRoadShowServicePlanEvent(client, clientUsers, address, _user, sale, _beginTime, _endTime,
					appointmentId));
		}

		/// <summary>
		/// 销售撤销预约
		/// </summary>
		/// <param name="appointmentId">预约标识</param>
		public void CancelAppointWithClient(Guid appointmentId)
		{
			ApplyAggregateEvent(new CancelAppointEvent(appointmentId));
			
			// 发送删除服务计划的事件
			RegisterDomainEvent(new CancelRoadShowServicePlanEvent(appointmentId));
		}

		#region ApplyMethods

		private void Apply(BindKeyIdeaAndTopicEvent @event)
		{
			_product = @event.Product;
		}

		private void Apply(CreateWeekSchedulerEvent @event)
		{
			_user = @event.User;
			_beginTime = @event.BeginTime;
			_endTime = @event.EndTime;
		}

		private void Apply(AddIdleDateTimeEvent @event)
		{
			_appointments.Add(@event.NewDateTime);
		}

		private void Apply(MakeAppointWithClientEvent @event)
		{
			var appointment = _appointments.FirstOrDefault(a => a.Id == @event.AppointmentId);
			Check.NotNull(appointment, nameof(appointment));
			appointment.MakeAppointWithClient(@event.Address, @event.ClientUsers, @event.Sale,
				@event.Description, @event.BookTime);
		}

		private void Apply(CancelAppointEvent @event)
		{
			var appointment = _appointments.FirstOrDefault(a => a.Id == @event.AppointmentId);
			Check.NotNull(appointment, nameof(appointment));
			
			_appointments.Remove(appointment);
		}

		private void Apply(RemoveIdleDateTimeEvent @event)
		{
			var appointment = _appointments.FirstOrDefault(a => a.Id == @event.AppointmentId);
			Check.NotNull(appointment, nameof(appointment));
			
			if (appointment.Booked)
			{
				throw new ServicePlanException("已预约,无法删除");
			}
			_appointments.Remove(appointment);
		}

		#endregion
	}
}
