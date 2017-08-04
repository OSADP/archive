using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using Repository.Providers.EntityFramework.Fakes;

namespace IDTO.UnitTests.Fake
{
    public class TravelerDbSet : FakeDbSet<Traveler>
    {
        public override Traveler Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<Traveler> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Traveler>(() => Find(keyValues));
        }
    }
    public class TripDbSet : FakeDbSet<Trip>
    {
        public override Trip Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<Trip> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Trip>(() => Find(keyValues));
        }
    }
    public class TripEventDbSet : FakeDbSet<TripEvent>
    {
        public override TripEvent Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TripEvent> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TripEvent>(() => Find(keyValues));
        }
    }

    public class ModeDbSet : FakeDbSet<Mode>
    {
        public override Mode Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<Mode> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Mode>(() => Find(keyValues));
        }
    }
    public class StepDbSet : FakeDbSet<Step>
    {
        public override Step Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<Step> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Step>(() => Find(keyValues));
        }
    }
    public class ProviderDbSet : FakeDbSet<Provider>
    {
        public override Provider Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<Provider> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Provider>(() => Find(keyValues));
        }
    }
    public class ProviderTypeDbSet : FakeDbSet<ProviderType>
    {
        public override ProviderType Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<ProviderType> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<ProviderType>(() => Find(keyValues));
        }
    }
    public class TConnectDbSet : FakeDbSet<TConnect>
    {
        public override TConnect Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TConnect> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TConnect>(() => Find(keyValues));
        }
    }
    public class TConnectOpportunityDbSet : FakeDbSet<TConnectOpportunity>
    {
        public override TConnectOpportunity Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TConnectOpportunity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TConnectOpportunity>(() => Find(keyValues));
        }
    }
    public class TConnectStatusDbSet : FakeDbSet<TConnectStatus>
    {
        public override TConnectStatus Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TConnectStatus> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TConnectStatus>(() => Find(keyValues));
        }
    }

    public class TConnectRequestDbSet : FakeDbSet<TConnectRequest>
    {
        public override TConnectRequest Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TConnectRequest> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TConnectRequest>(() => Find(keyValues));
        }
    }

    public class TConnectedVehicleDbSet : FakeDbSet<TConnectedVehicle>
    {
        public override TConnectedVehicle Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TConnectedVehicle> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TConnectedVehicle>(() => Find(keyValues));
        }
    }

    public class LastVehiclePositionDbSet : FakeDbSet<LastVehiclePosition>
    {
        public override LastVehiclePosition Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<LastVehiclePosition> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<LastVehiclePosition>(() => Find(keyValues));
        }
    }
    public class TravelerLocationDbSet : FakeDbSet<TravelerLocation>
    {
        public override TravelerLocation Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (int)keyValues.FirstOrDefault());
        }
        public override Task<TravelerLocation> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<TravelerLocation>(() => Find(keyValues));
        }
    }
    public class BlockDbSet : FakeDbSet<Block>
    {
        public override Block Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.Id == (string)keyValues.FirstOrDefault());
        }
        public override Task<Block> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Block>(() => Find(keyValues));
        }
    }

}
