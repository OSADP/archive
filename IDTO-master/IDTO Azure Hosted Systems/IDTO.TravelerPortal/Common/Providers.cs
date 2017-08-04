using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Common
{
    public class ProviderType
    {
        public enum ProviderId
        {
            FixedRoute_TConnect = 1,
            IncomingFixedRoute = 2,
            Rideshare = 3,
            Demand_Response = 4
        }
    }

    public sealed class Providers
    {
        private readonly int m_id;
        private readonly string m_name;
        private readonly ProviderType.ProviderId m_type;

        private Providers(int id, string name, ProviderType.ProviderId type)
        {
            m_id = id;
            m_name = name;
            m_type = type;
        }

        private static Providers COTA = new Providers(1, "COTA", ProviderType.ProviderId.FixedRoute_TConnect);
        private static Providers CABS = new Providers(2, "CABS", ProviderType.ProviderId.IncomingFixedRoute);
        private static Providers Zimride = new Providers(3, "Zimride", ProviderType.ProviderId.Rideshare);
        private static Providers CapTrans = new Providers(4, "CapTrans", ProviderType.ProviderId.Demand_Response);
        private static Providers BattelleFixed = new Providers(1001, "Battelle Fixed", ProviderType.ProviderId.FixedRoute_TConnect);
        private static Providers BattelleTaxi = new Providers(1002, "Battelle Taxi", ProviderType.ProviderId.Demand_Response);
        private static Providers CFRTA = new Providers(1003, "CFRTA", ProviderType.ProviderId.FixedRoute_TConnect);
        private static Providers FDOT = new Providers(1004, "FDOT", ProviderType.ProviderId.FixedRoute_TConnect);
        private static Providers UCF = new Providers(2003, "UCF", ProviderType.ProviderId.IncomingFixedRoute);

        public static string IdToString(int id)
        {
            string returnVal = "";
            if (COTA.m_id == id)
            {
                returnVal = COTA.m_name;
            }
            else if (CABS.m_id == id)
            {
                returnVal = CABS.m_name;
            }
            else if (Zimride.m_id == id)
            {
                returnVal = Zimride.m_name;
            }
            else if (CapTrans.m_id == id)
            {
                returnVal = CapTrans.m_name;
            }
            else if (BattelleFixed.m_id == id)
            {
                returnVal = BattelleFixed.m_name;
            }
            else if (BattelleTaxi.m_id == id)
            {
                returnVal = BattelleTaxi.m_name;
            }
            else if (CFRTA.m_id == id)
            {
                returnVal = CFRTA.m_name;
            }
            else if (FDOT.m_id == id)
            {
                returnVal = CFRTA.m_name;
            }
            else if (UCF.m_id == id)
            {
                returnVal = UCF.m_name;
            }
            else
            {
                returnVal = "";
            }
            return returnVal;
        }

        public static int? StringToId(string providerName)
        {
            int? returnVal = null;

            if (providerName != null)
            {
                if (COTA.m_name.Equals(providerName))
                {
                    returnVal = COTA.m_id;
                }
                else if (CABS.m_name.Equals(providerName))
                {
                    returnVal = (int)CABS.m_id;
                }
                else if (Zimride.m_name.Equals(providerName))
                {
                    returnVal = (int)Zimride.m_id;
                }
                else if (CapTrans.m_name.Equals(providerName))
                {
                    returnVal = (int)CapTrans.m_id;
                }
                else if (BattelleFixed.m_name.Equals(providerName))
                {
                    returnVal = (int)BattelleFixed.m_id;
                }
                else if (BattelleTaxi.m_name.Equals(providerName))
                {
                    returnVal = (int)BattelleTaxi.m_id;
                }
                else if (CFRTA.m_name.Equals(providerName))
                {
                    returnVal = (int)CFRTA.m_id;
                }
                else if (FDOT.m_name.Equals(providerName))
                {
                    returnVal = (int)FDOT.m_id;
                }
                else if (UCF.m_name.Equals(providerName))
                {
                    returnVal = (int)UCF.m_id;
                }
                else
                {
                    returnVal = 0;
                }
            }
            return returnVal;
        }
    }
}