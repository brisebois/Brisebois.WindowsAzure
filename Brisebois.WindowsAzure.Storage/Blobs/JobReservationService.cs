using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Brisebois.WindowsAzure.Storage.Blobs
{
public class JobReservationService : IDisposable
{
    private readonly string reserverName;

    private readonly BlobLeaseManager manager;
       
    private CloudBlobContainer container;

    public JobReservationService(string connectionString, 
                                    string containerName, 
                                    string reserverName)
    {
        this.reserverName = reserverName;
        Init(connectionString, containerName);

        manager =  new BlobLeaseManager();
    }

    private void Init(string connectionString, string containerName)
    {
        var cs = CloudConfigurationManager.GetSetting(connectionString);

        var account = CloudStorageAccount.Parse(cs);

        var client = account.CreateCloudBlobClient();

        container = client.GetContainerReference(containerName);
        container.CreateIfNotExists();
    }

    public bool TryReserveJob(string jobName, double jobReservationInSeconds)
    {
        var blobReference = GetJobReservationBlob(jobName);
            
        if(!blobReference.Exists())
            InitializeLeaseBlob(blobReference);

        var acquireLease = manager.TryAcquireLease(blobReference, jobReservationInSeconds);
            
        if(acquireLease)
            UpdateReservationLog(jobName);

        return acquireLease;
    }

    public bool HasReservation(string jobName)
    {
        return manager.HasLease(GetJobReservationBlob(jobName));
    }

    public void CancelReservation(string jobName)
    {
        manager.ReleaseLease(GetJobReservationBlob(jobName));
    }

    private CloudBlockBlob GetJobReservationBlob(string jobName)
    {
        var blobReference = container.GetBlockBlobReference(jobName);
        return blobReference;
    }

    private void InitializeLeaseBlob(CloudBlockBlob blobReference)
    {
        var log = new JobReservationLog();
        UpdateBlobContent(log, blobReference);
    }

    private void UpdateBlobContent(JobReservationLog jobReservationLog, 
                                    CloudBlockBlob jobReservationBlob)
    {
        jobReservationLog.Add(MakeJobReservation());

        string leaseId = manager.GetLeaseId(jobReservationBlob);

        AccessCondition accessCondition = string.IsNullOrWhiteSpace(leaseId)
            ? null
            : new AccessCondition
            {
                LeaseId = leaseId
            };

        jobReservationBlob.UploadText(jobReservationLog.ToJson(), 
                                        null, 
                                        accessCondition);
    }

    private void UpdateReservationLog(string jobName)
    {
        CloudBlockBlob blobReference = GetJobReservationBlob(jobName);

        JobReservationLog jobReservationLog = JobReservationLog.Make(blobReference.DownloadText());
        JobReservation lastReservation = jobReservationLog.LastReservation;

        if (lastReservation.Reserver == reserverName) 
            return;
            
        UpdateBlobContent(jobReservationLog, blobReference);
    }

    private JobReservation MakeJobReservation()
    {
        return new JobReservation
        {
            Obtained = DateTime.UtcNow,
            Reserver = reserverName,
        };
    }
        
    public void Dispose()
    {
        manager.Dispose();
    }

    private struct JobReservation
    {
        public string Reserver { get; set; }
        public DateTime Obtained { get; set; }
    }

    private class JobReservationLog
    {
        private readonly List<JobReservation> reservations = new List<JobReservation>();

        public JobReservationLog()
        {
        }

        private JobReservationLog(List<JobReservation> lockReservations)
        {
            reservations = lockReservations;
        }

        internal JobReservation LastReservation
        {
            get { return reservations.FirstOrDefault(); }
        }

        internal static JobReservationLog Make(string json)
        {
            var list = JsonConvert.DeserializeObject<List<JobReservation>>(json);
            return new JobReservationLog(list);
        }

        internal void Add(JobReservation jobReservation)
        {
            reservations.Insert(0, jobReservation);

            if (reservations.Count > 10)
                reservations.Remove(reservations.Last());
        }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(reservations);
        }
    }
}
}