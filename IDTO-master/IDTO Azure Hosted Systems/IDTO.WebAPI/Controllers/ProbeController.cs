using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.ServiceRuntime;
using IDTO.Entity.Models;
using IDTO.Data;
using IDTO.Common.Storage;
using Microsoft.WindowsAzure.Storage;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
//using System.ServiceModel.Web;
//using System.Web.Http.Tracing;
using System.Configuration;
namespace IDTO.WebAPI.Controllers
{
    [Authorize]
    public class ProbeController : BaseController
    {
        private readonly IAzureTable<ProbeSnapshotEntry> _probeTable;
        private const string WadConnectionString = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeController"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ProbeController(IDbContext context)
            : base(context)
        {
            //Get the Azure Storage Account
            string connection = ConfigurationManager.ConnectionStrings[WadConnectionString].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection);

            _probeTable = new AzureTable<ProbeSnapshotEntry>(storageAccount);
            _probeTable.CreateIfNotExist();

       

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeController"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="probeTable">The probe table.</param>
        public ProbeController(IDbContext context, IAzureTable<ProbeSnapshotEntry> probeTable)
            : base(context)
        {
            _probeTable = probeTable;
        }

        /// <summary>
        /// Receives ProbeVehicleData by inbound vehicle for the latest vehicle location and saves it to the cloud.
        /// </summary>
        /// <param name="probeDataMessage"></param>
        /// <returns></returns>
        public HttpResponseMessage PostProbeData(ProbeVehicleData probeDataMessage)
        {
            try
            {
                if (String.IsNullOrEmpty(probeDataMessage.InboundVehicle))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InboundVehicle cannot be null or empty");
                }
             
                // Convert from Java timestamp
                DateTime dtTemp = new DateTime(1970, 1, 1, 0, 0, 0);

                DateTime newestProbeTimestamp = dtTemp;
                ProbeSnapshotEntry newestProbeSnapshot = null;

                foreach (PositionSnapshot positionSnapshot in probeDataMessage.Positions)
                {
                    DateTime lastUpdatedDate = dtTemp.AddMilliseconds(positionSnapshot.TimeStamp);

                    ProbeSnapshotEntry newProbeSnapshot = new ProbeSnapshotEntry
                        {
                            PartitionKey = probeDataMessage.InboundVehicle,
                            RowKey = Guid.NewGuid().ToString(),
                            Latitude = positionSnapshot.Latitude,
                            Longitude = positionSnapshot.Longitude,
                            Speed = positionSnapshot.Speed,
                            Heading = positionSnapshot.Heading,
                            PositionTimestamp = lastUpdatedDate,
                            Satellites = positionSnapshot.Satellites,
                            Accuracy = positionSnapshot.Accuracy,
                            Altitude = positionSnapshot.Altitude
                        };

                    _probeTable.AddEntity(newProbeSnapshot);

                    //Check for and hang on to most recent probe snapshot
                    if (lastUpdatedDate > newestProbeTimestamp)
                    {
                        newestProbeTimestamp = lastUpdatedDate;
                        newestProbeSnapshot = newProbeSnapshot;
                    }
                }

                SaveNewestProbeSnapshot(newestProbeSnapshot);

                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "ProbeController.PostProbeData");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message + msg);
            }

        }

        private void SaveNewestProbeSnapshot(ProbeSnapshotEntry newestProbeSnapshot)
        {
            if (newestProbeSnapshot == null) return;

            //Find existing vehicle record
            LastVehiclePosition currentVehicle = Uow.Repository<LastVehiclePosition>().Query().Get().FirstOrDefault(v => v.VehicleName == newestProbeSnapshot.PartitionKey);

            if (currentVehicle != null)
            {
                //Update vehicles last position values
                currentVehicle.PositionTimestamp = newestProbeSnapshot.PositionTimestamp;
                currentVehicle.Latitude = newestProbeSnapshot.Latitude;
                currentVehicle.Longitude = newestProbeSnapshot.Longitude;
                currentVehicle.Speed = newestProbeSnapshot.Speed;
                currentVehicle.Heading = (short) newestProbeSnapshot.Heading;
                currentVehicle.Accuracy = newestProbeSnapshot.Accuracy;

                Uow.Repository<LastVehiclePosition>().Update(currentVehicle);
            }
            else
            {
                LastVehiclePosition lvp = new LastVehiclePosition
                {
                    VehicleName = newestProbeSnapshot.PartitionKey,
                    PositionTimestamp = newestProbeSnapshot.PositionTimestamp,
                    Latitude = newestProbeSnapshot.Latitude,
                    Longitude = newestProbeSnapshot.Longitude,
                    Speed = newestProbeSnapshot.Speed,
                    Heading = (short) newestProbeSnapshot.Heading,
                    Accuracy = newestProbeSnapshot.Accuracy
                };

                Uow.Repository<LastVehiclePosition>().Insert(lvp);
            }

            Uow.Save();
        }
    }
}
