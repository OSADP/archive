using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MapEdit.Data.Models
{
	public class IncZoneMapContext : DbContext
	{
		public DbSet<mapLink> mapLinks { get; set; }
		public DbSet<mapNode> mapNodes { get; set; }
		public DbSet<mapSet> mapSets { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{

			//modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
			//modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			//modelBuilder.Conventions.Remove<ManyToOneCascadeDeleteConvention>();
		}
	}

	public class Model
	{
	}

	public class mapLink
	{
		[Key]
		public Guid Id { get; set; }
		public Guid? mapSetId { get; set; }
		public virtual mapSet mapSet { get; set; }

		public Guid? startMapNodeId { get; set; }
		public virtual mapNode startMapNode { get; set; }
		public Guid? endMapNodeId { get; set; }
		public virtual mapNode endMapNode { get; set; }
	}

	public class mapNode
	{
//		public int Id { get; set; }
		[Key]
		public Guid Id { get; set; }
		public double latitude { get; set; }
		public double longitude { get; set; }
		public double elevation { get; set; }
		public int laneWidth { get; set; }
		public int directionality { get; set; }
		public int xOffset { get; set; }
		public int yOffset { get; set; }
		public int zOffset { get; set; }
		public int positionalAccuracyP1 { get; set; }
		public int positionalAccuracyP2 { get; set; }
		public int positionalAccuracyP3 { get; set; }
		public int laneOrder { get; set; }
		public int postedSpeed { get; set; }
		public Guid mapSetId { get; set; }
		public virtual mapSet mapSet { get; set; }
		public int distance { get; set; }
		public string LaneDirection { get; set; }
		public string LaneType { get; set; }
	}

	public class mapSet
	{
		[Key]
		public Guid Id { get; set; }
		public string name { get; set; }
		public string description { get; set; }

	}
}