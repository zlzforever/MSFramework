using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MSFramework.CQRS;
using Ordering.Domain.Aggregates;


namespace Ordering.API.Application.Command
{

    public class CreateOrderCommand : ICommand
    {
        public string Description { get; set; }

        public string Address { get; set; }

        public List<OrderItem> OrderItems { get; set; }

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }
	}
}
