using System;
using System.Collections.Generic;
using MSFramework.Data;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 可路演时间
	/// </summary>
	public class Appointment : EntityBase<Guid>
	{
		/// <summary>
		/// 是否预定
		/// </summary>
		private bool _booked;
		
		private DateTime _beginTime;
		
		private DateTime _endTime;
		
		private DateTime _bookTime;

		/// <summary>
		/// 可路演地点
		/// </summary>
		private string _location;

		/// <summary>
		/// 地址
		/// </summary>
		private string _address;

		/// <summary>
		/// 客户联系人
		/// </summary>
		private List<ClientUser> _clientUsers;

		/// <summary>
		/// 销售
		/// </summary>
		private User _sale;
		
		/// <summary>
		/// 描述
		/// </summary>
		private string _description;

		public bool Booked => _booked;

		public Appointment(string location, DateTime beginTime, DateTime endTime)
		{
			_location = location;
			_bookTime = beginTime;
			_endTime = endTime;
			_booked = false;
		}

		public void MakeAppointWithClient(string address, List<ClientUser> clientUsers, User sale, string description, DateTime bookTime)
		{
			if (_booked)
			{
				throw new ServicePlanException("该时间段已被预约");
			}
			
			_address = address;
			_clientUsers = clientUsers;
			_sale = sale;
			_bookTime = bookTime;
			_description = description;
			_booked = true;
		}
	}
}