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
		private bool _isBooked;
		
		private DateTime _beginTime;
		
		private DateTime _endTime;
		
		private DateTime _bookTime;

		/// <summary>
		/// 地址
		/// </summary>
		private string _address;
		
		/// <summary>
		/// 客户
		/// </summary>
		private Client _client;

		/// <summary>
		/// 客户联系人
		/// </summary>
		private List<ClientUser> _clientUsers;

		/// <summary>
		/// 销售
		/// </summary>
		private Sale _sale;
		
		/// <summary>
		/// 描述
		/// </summary>
		private string _description;

		public bool IsBooked => _isBooked;

		public Appointment(DateTime beginTime, DateTime endTime)
		{
			_bookTime = beginTime;
			_endTime = endTime;
			_isBooked = false;
		}

		public void MakeAppointWithClient(string address, Client client, List<ClientUser> clientUsers, Sale sale, string description, DateTime bookTime)
		{
			if (_isBooked)
			{
				throw new ServicePlanException("该时间段已被预约");
			}
			
			_address = address;
			_client = client;
			_clientUsers = clientUsers;
			_sale = sale;
			_bookTime = bookTime;
			_description = description;
			_isBooked = true;
		}
	}
}