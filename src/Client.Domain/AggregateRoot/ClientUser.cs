using System;
using MSFramework.Domain;

namespace Client.Domain.AggregateRoot.ClientAggregateRoot
{
	public class ClientUser: AggregateRootBase
	{
		/// <summary>
		/// 客户ID
		/// </summary>
		private Guid ClientId;

		/// <summary>
		/// 名
		/// </summary>
		private string FirstName;

		/// <summary>
		/// 姓
		/// </summary>
		private string LastName;

		/// <summary>
		/// 称谓
		/// </summary>
		private string Civility;

		/// <summary>
		/// 职位
		/// </summary>
		private string Title;

		/// <summary>
		/// 邮箱
		/// </summary>
		private string Email;

		/// <summary>
		/// 座机
		/// </summary>
		private string Phone;

		/// <summary>
		/// 激活状态
		/// </summary>
		private bool Active;

		/// <summary>
		/// 电话
		/// </summary>
		private string Mobile;

		/// <summary>
		/// 职位描述
		/// </summary>
		private string TitleDescription;

		/// <summary>
		/// 行业描述
		/// </summary>
		private string IndustryDescription;

		/// <summary>
		/// 部门
		/// </summary>
		private string Department;

		/// <summary>
		/// 部门
		/// </summary>
		private string DepartmentDescription;

		/// <summary>
		/// 打分权重
		/// </summary>
		private int ScoringPriority;


		public Guid getClientId()
		{
			return ClientId;
		}

		public void setClientId(Guid clientId)
		{
			ClientId = clientId;
		}

		public string getFirstName()
		{
			return FirstName;
		}

		public void setFirstName(string firstName)
		{
			FirstName = firstName;
		}

		public string getLastName()
		{
			return LastName;
		}

		public void setLastName(string lastName)
		{
			LastName = lastName;
		}

		public string getCivility()
		{
			return Civility;
		}

		public void setCivility(string civility)
		{
			Civility = civility;
		}

		public string getTitle()
		{
			return Title;
		}

		public void setTitle(string title)
		{
			Title = title;
		}

		public string getEmail()
		{
			return Email;
		}

		public void setEmail(string email)
		{
			Email = email;
		}

		public string getPhone()
		{
			return Phone;
		}

		public void setPhone(string phone)
		{
			Phone = phone;
		}

		public bool isActive()
		{
			return Active;
		}

		public void setActive(bool active)
		{
			Active = active;
		}

		public string getMobile()
		{
			return Mobile;
		}

		public void setMobile(string mobile)
		{
			Mobile = mobile;
		}

		public string getTitleDescription()
		{
			return TitleDescription;
		}

		public void setTitleDescription(string titleDescription)
		{
			TitleDescription = titleDescription;
		}

		public string getIndustryDescription()
		{
			return IndustryDescription;
		}

		public void setIndustryDescription(string industryDescription)
		{
			IndustryDescription = industryDescription;
		}

		public string getDepartment()
		{
			return Department;
		}

		public void setDepartment(string department)
		{
			Department = department;
		}

		public string getDepartmentDescription()
		{
			return DepartmentDescription;
		}

		public void setDepartmentDescription(string departmentDescription)
		{
			DepartmentDescription = departmentDescription;
		}

		public int getScoringPriority()
		{
			return ScoringPriority;
		}

		public void setScoringPriority(int scoringPriority)
		{
			ScoringPriority = scoringPriority;
		}
	}
}