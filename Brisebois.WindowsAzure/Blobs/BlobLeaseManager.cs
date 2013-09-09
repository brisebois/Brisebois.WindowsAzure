using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
public class BlobLeaseManager : IDisposable
{
    private const string FAILED_ATTEMPT_AT_RELEASING_BLOB_LEASE =
        @"|> failed attempt at releasing blob lock on {0} using lease id {1}";

    private const string FAILED_TO_ACQUIRE_LEASE = @"|> failed to acquire lease {0}";

    private const string REMOVED_LEASE_FROM_ACQUIREDLEASES
        = @"|> removed lease from acquired leases for blob {0}";

    private const string RENEWED_LEASE = @"|> renewed lease for {0}";

    private readonly Dictionary<string, Lease> acquiredLeases = new Dictionary<string, Lease>();

    public void Dispose()
    {
        acquiredLeases.ForEach(pair => pair.Value.KeepAlive.Dispose());
    }

    public bool HasLease(CloudBlockBlob blob)
    {
        return acquiredLeases.ContainsKey(blob.Name);
    }

    public string GetLeaseId(CloudBlockBlob blob)
    {
        return HasLease(blob) ? acquiredLeases[blob.Name].LeaseId : string.Empty;
    }

    public void ReleaseLease(CloudBlockBlob blob)
    {
        if (!HasLease(blob)) return;

        string leaseId = GetLeaseId(blob);

        ClearLease(blob);

        try
        {
            blob.ReleaseLease(new AccessCondition
            {
                LeaseId = leaseId
            });
        }
        catch (StorageException)
        {
            Trace.WriteLine(string.Format(FAILED_ATTEMPT_AT_RELEASING_BLOB_LEASE,
                                          blob.Name,
                                          leaseId));
        }
    }

    public bool TryAcquireLease(CloudBlockBlob blob, double leaseTimeInSeconds)
    {
        if(leaseTimeInSeconds < 15 || leaseTimeInSeconds > 60)
            throw new ArgumentException(@"value must be greater than 15 and smaller than 60", 
                                        "leaseTimeInSeconds");
        try
        {
            string proposedLeaseId = Guid.NewGuid().ToString();

            var leaseTime = TimeSpan.FromSeconds(leaseTimeInSeconds);
            string leaseId = blob.AcquireLease(leaseTime,
                                                        proposedLeaseId);

            UpdateAcquiredLease(blob, leaseId, leaseTimeInSeconds);
                
            return true;
        }
        catch (StorageException)
        {
            Trace.WriteLine(string.Format(FAILED_TO_ACQUIRE_LEASE, blob.Name));
            return false;
        }
    }

    private void UpdateAcquiredLease(CloudBlockBlob blob,
                                     string leaseId,
                                     double lockTimeInSeconds)
    {
        var name = blob.Name;

        if (IsAcquiredLeaseMissMatched(name, leaseId))
            ClearLease(blob);
        else
            acquiredLeases.Add(name, MakeLease(blob, leaseId, lockTimeInSeconds));
    }

    private bool IsAcquiredLeaseMissMatched(string name, string leaseId)
    {
        return acquiredLeases.ContainsKey(name) &&
                acquiredLeases[name].LeaseId != leaseId;
    }

    private void ClearLease(CloudBlockBlob blob)
    {
        var name = blob.Name;

        Lease lease = acquiredLeases[name];
        lease.KeepAlive.Dispose();
        acquiredLeases.Remove(name);
    }

    private Lease MakeLease(CloudBlockBlob blob,
                            string leaseId,
                            double lockTimeInSeconds)
    {
        TimeSpan interval = TimeSpan.FromSeconds(lockTimeInSeconds - 1);

        return new Lease
        {
            LeaseId = leaseId,
            KeepAlive = Observable.Interval(interval)
                .Subscribe(l => RenewLease(blob))
        };
    }

    private bool RenewLease(CloudBlockBlob blob)
    {
        if (!HasLease(blob))
            return false;

        var name = blob.Name;

        try
        {
            blob.RenewLease(new AccessCondition
            {
                LeaseId = acquiredLeases[name].LeaseId
            });

            Trace.WriteLine(string.Format(RENEWED_LEASE, name));

            return true;
        }
        catch (StorageException)
        {
            acquiredLeases.Remove(name);

            Trace.WriteLine(string.Format(REMOVED_LEASE_FROM_ACQUIREDLEASES, name));

            return false;
        }
    }

    private struct Lease
    {
        internal string LeaseId { get; set; }
        internal IDisposable KeepAlive { get; set; }
    }
}
}