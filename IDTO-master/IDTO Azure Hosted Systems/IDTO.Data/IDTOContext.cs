using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
//using Repository;
using Repository.Providers.EntityFramework;

namespace IDTO.Data
{
    public class IDTOContext : DbContextBase
    {
        public DbSet<WebApiGetTripUsage> WebApiGetTripUsages { get; set; }
        public DbSet<Dispatcher> Dispatchers { get; set; }
        public DbSet<Traveler> Travelers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripEvent> TripEvents { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Mode> Modes { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<ProviderType> ProviderTypes { get; set; }
        public DbSet<TConnect> TConnects { get; set; }
        public DbSet<TConnectOpportunity> TConnectOpportunities { get; set; }
        public DbSet<TConnectStatus> TConnectStatuses { get; set; }
        public DbSet<TConnectRequest> TConnectRequests { get; set; }
        public DbSet<TConnectedVehicle> TConnectedVehicles { get; set; }
        public DbSet<LastVehiclePosition> LastVehiclePositions { get; set; }
        public DbSet<TravelerLocation> TravelerLocations { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }

        public IDTOContext(string connectionString) : base(connectionString)
        {
           Database.SetInitializer<IDTOContext>(null);
         // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<IDTOContext>());
            Configuration.ProxyCreationEnabled = false; 
        }
        ///Ninject seems to use this default constructor, if available, over the connectionString one above.
        ///However, the webapi scaffolding requires this default constructor to wizard up new controllers for us.
        public IDTOContext()
            : base("IDTOContext")
        {
            Database.SetInitializer<IDTOContext>(null);
            Configuration.ProxyCreationEnabled = false;
        }

        //public new IDbSet<T> Set<T>() where T : class
        //{
        //    return base.Set<T>();
        //}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Configurations.Add(new CustomerMap());
        }
    }
}
