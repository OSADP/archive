using System;
using SQLite;

namespace IDTO.Common.Models {

	public class FavoriteLocation {
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		public string Location { get; set; }
	}
}

