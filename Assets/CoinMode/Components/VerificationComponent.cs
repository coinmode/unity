using CoinMode.NetApi;
using System.Collections.Generic;

namespace CoinMode
{
    public enum VerificationStatus
    {
        None,
        Pending,
        Approved,
    }

    public class VerificationMethod
    {
        public VerificationMethodProperties properties { get; private set; } = null;
        public VerificationStatus status { get; internal set; } = VerificationStatus.None;
        internal string verificationKey { get; set; } = null;
        public bool clientAuthorisable
        {
            get
            {
                return properties.type != "none";
            }
        }

        internal VerificationMethod(VerificationMethodProperties properties)
        {
            this.properties = properties;
            status = VerificationStatus.Pending;
            verificationKey = "";
        }
    }

    public class VerificationComponent : CoinModeComponent
    {
        public enum Status
        {
            None,
            Pending,
            PartApproved,
            Approved,
            Error,
        }

        public Status state { get; private set; } = Status.None;

        public IEnumerable<KeyValuePair<string, VerificationMethod>> requiredMethods
        { 
            get
            {
                foreach (KeyValuePair<string, VerificationMethod> pair in _requiredMethods)
                {
                    yield return pair;
                }
            }
        }
        private Dictionary<string, VerificationMethod> _requiredMethods = new Dictionary<string, VerificationMethod>();

        public IEnumerable<KeyValuePair<string, string>> errors
        {
            get
            {
                foreach (KeyValuePair<string, string> pair in _errors)
                {
                    yield return pair;
                }
            }
        }
        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        public int requiredCount
        {
            get { return _requiredMethods.Count; }
        }

        public int approvedCount { get; private set; } = 0;

        internal VerificationComponent() { }

        internal void Clear()
        {
            _requiredMethods.Clear();
            approvedCount = 0;
            state = Status.None;
        }

        internal void SetUp(VerificationMethodProperties[] requiredVerification)
        {
            for (int i = 0; i < requiredVerification.Length; i++)
            {
                VerificationMethod verification = new VerificationMethod(requiredVerification[i]);
                _requiredMethods.Add(verification.properties.field, verification);
            }
            approvedCount = 0;
            state = Status.Pending;
        }

        public bool SetKey(string field, string key)
        {
            VerificationMethod verification;
            if (_requiredMethods.TryGetValue(field, out verification))
            {
                if (verification.clientAuthorisable)
                {
                    verification.verificationKey = key;
                }
                else
                {
                    CoinModeLogging.LogWarning("VerificationComponent", "SetVerificationKey", "Cannot verify method {0} from client.", field.ToString());
                }
            }
            else
            {
                CoinModeLogging.LogWarning("VerificationComponent", "SetVerificationKey", "Verification method {0} not required.", field.ToString());
            }
            return false;
        }

        internal void UpdateFromStatus(Dictionary<string, string> verificationStatus)
        {
            foreach (KeyValuePair<string, VerificationMethod> verification in _requiredMethods)
            {
                if (verification.Value.status != VerificationStatus.Approved)
                {
                    string newStatus;
                    if (verificationStatus.TryGetValue(verification.Key, out newStatus))
                    {
                        verification.Value.status = newStatus == "approved" ? VerificationStatus.Approved : VerificationStatus.Pending;
                        if(verification.Value.status == VerificationStatus.Approved)
                        {
                            approvedCount++;
                        }                        
                    }
                }
            }

            if(approvedCount == requiredCount)
            {
                state = Status.Approved;
            }
            else if(approvedCount > 0)
            {                
                if(approvedCount > requiredCount)
                {
                    CoinModeLogging.LogWarning("VerificationComponent", "UpdateFromStatus", "Approved verification count ({0}) exceeds required verification count ({1}), this is unexpected!", 
                        approvedCount, requiredCount);
                    state = Status.Error;
                }
                else
                {
                    state = Status.PartApproved;
                }
            }
        }

        internal void UpdateErrors(string[] verificationErrors)
        {
            _errors.Clear();
            if(verificationErrors != null)
            {
                for (int i = 0; i < verificationErrors.Length; i++)
                {
                    VerificationMethod method;
                    if (_requiredMethods.TryGetValue(verificationErrors[i], out method))
                    {
                        _errors.Add(verificationErrors[i], method.properties.userText);
                    }
                    else
                    {
                        _errors.Add(verificationErrors[i], verificationErrors[i] + " verification error!");
                    }
                }
            }   
        }

        internal void UpdateErrors(Dictionary<string, string> verificationErrors)
        {
            _errors.Clear();
            foreach(KeyValuePair<string, string> pair in verificationErrors)
            {
                _errors[pair.Key] = pair.Value;
            }
        }

        internal Dictionary<string, string> GetKeys()
        {
            Dictionary<string, string> keys = new Dictionary<string, string>(_requiredMethods.Count);
            foreach(KeyValuePair<string, VerificationMethod> pair in _requiredMethods)
            {
                keys.Add(pair.Key, pair.Value.verificationKey);
            }
            return keys;
        }
    }
}