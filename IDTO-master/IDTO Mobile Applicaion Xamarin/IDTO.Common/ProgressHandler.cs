using System;

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace IDTO.Common
{
    public class ProgressHandler : DelegatingHandler
    {
        int busyCount = 0;

        public event Action<bool> BusyStateChange;

        #region implemented abstract members of HttpMessageHandler

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            //assumes always executes on UI thread
            if (busyCount++ == 0 && BusyStateChange != null)
                BusyStateChange(true);

            var response = await base.SendAsync(request, cancellationToken);

            // assumes always executes on UI thread
            if (--busyCount == 0 && BusyStateChange != null)
                BusyStateChange(false);

            return response;
        }

        #endregion

    }
}