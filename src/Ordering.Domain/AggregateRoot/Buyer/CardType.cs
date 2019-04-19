using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot.Buyer
{
	/// <remarks> 
	/// Card type class should be marked as abstract with protected constructor to encapsulate known enum types
	/// this is currently not possible as OrderingContextSeed uses this constructor to load cardTypes from csv file
	/// </remarks>
	public class CardType
		: Enumeration
	{
		public static CardType Amex = new CardType(1, "Amex");
		public static CardType Visa = new CardType(2, "Visa");
		public static CardType MasterCard = new CardType(3, "MasterCard");

		public CardType(int id, string name)
			: base(id, name)
		{
		}

		public static IEnumerable<CardType> All => new[]
		{
			Amex,
			Visa,
			MasterCard
		};

		public static CardType From(int id)
		{
			var state = All.SingleOrDefault(s => s.Id == id);

			if (state == null)
			{
				throw new OrderingException(
					$"Possible values for CardType: {String.Join(",", All.Select(s => s.Name))}");
			}

			return state;
		}
	}
}