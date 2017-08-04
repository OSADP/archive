using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using Repository.Providers.EntityFramework.Fakes;


namespace IDTO.UnitTests.Fake
{
    public class IDTOFakeContext : FakeDbContext
    {
        public IDTOFakeContext()
        {
            AddFakeDbSet<Traveler, TravelerDbSet>();
            AddFakeDbSet<Trip, TripDbSet>();
            AddFakeDbSet<TripEvent, TripEventDbSet>();
            AddFakeDbSet<Step, StepDbSet>();
            AddFakeDbSet<Mode, ModeDbSet>();
            AddFakeDbSet<Provider, ProviderDbSet>();
            AddFakeDbSet<ProviderType, ProviderTypeDbSet>(); 
            AddFakeDbSet<TConnect, TConnectDbSet>();
            AddFakeDbSet<TConnectOpportunity, TConnectOpportunityDbSet>();
            AddFakeDbSet<TConnectStatus, TConnectStatusDbSet>();
            AddFakeDbSet<TConnectRequest, TConnectRequestDbSet>();
            AddFakeDbSet<TConnectedVehicle, TConnectedVehicleDbSet>();
            AddFakeDbSet<LastVehiclePosition, LastVehiclePositionDbSet>();
            AddFakeDbSet<TravelerLocation, TravelerLocationDbSet>();
            AddFakeDbSet<Block, BlockDbSet>();
        }
    }
}
