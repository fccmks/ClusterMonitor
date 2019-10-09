using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Fabric;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace ClusterMonitoring
{
    public class ClusterInfo
    {
        public string ClusterName { get; set; }
        public string ConnectionEndpoint { get; set; }
        public string CodeVersion { get; set; }

        public ClusterInfo(string name)
        {
            ClusterName = name;
        }
    }

    public class SFConnector
    {
        public List<ClusterInfo> Clusters { get; private set; }

        public void FillTable(string pathToSettings)
        {
            Clusters = new List<ClusterInfo>();
            string json;

            using (var r = new StreamReader(pathToSettings))
            {
                json = r.ReadToEnd();
                r.Close();
            }

            var dataSet = JsonConvert.DeserializeObject<DataSet>(json);
            var dataTable = dataSet.Tables["Clusters"];

            foreach (DataRow row in dataTable.Rows)
            {
                var sc = new X509Credentials
                {
                    FindType = X509FindType.FindByThumbprint,
                    StoreLocation = StoreLocation.LocalMachine,
                    StoreName = "My",
                    FindValue = row["clientCertThumb"]
                };
                sc.RemoteCertThumbprints.Add(row["serverCertThumb"].ToString());

                var item = new ClusterInfo(row["name"].ToString())
                {
                    ConnectionEndpoint = row["ConnectionEndpoint"].ToString()
                };

                try
                {
                    using (var fc = new FabricClient(sc, item.ConnectionEndpoint))
                    {
                        var clusterCodeVersion = fc.ClusterManager
                            .GetFabricUpgradeProgressAsync(TimeSpan.FromSeconds(5), CancellationToken.None).Result;
                        item.CodeVersion = clusterCodeVersion.TargetCodeVersion;
                    }
                }
                catch (Exception e)
                {
                    item.CodeVersion = "Connection timeout.";
                }

                Clusters.Add(item);
            }
        }
    }
}
