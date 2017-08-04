using System;
using System.Collections.Generic;

using IDTO.Common;
using IDTO.Common.Models;

namespace IDTO.Android
{
	public class FavoritesRepository
	{
		FavoritesDbManager db = null;
		protected static FavoritesRepository me;	
		static FavoritesRepository ()
		{
			me = new FavoritesRepository();
		}
		protected FavoritesRepository()
		{
			db = new FavoritesDbManager(FavoritesDbManager.DatabaseFilePath);
		}

		public static FavoriteLocation GetFavoriteLocation(int id)
		{
			return me.db.GetFavoriteLocation(id);
		}

		public static IEnumerable<FavoriteLocation> GetFavoriteLocations ()
		{
			return me.db.GetFavoriteLocations();
		}

		public static int SaveFavoriteLocation (FavoriteLocation item)
		{
			return me.db.SaveFavoriteLocation(item);
		}

		public static int DeleteFavoriteLocation(FavoriteLocation item)
		{
			return me.db.DeleteFavoriteLocation(item);
		}
	}
}

