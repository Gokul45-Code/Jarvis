namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System.Runtime.Serialization;

#pragma warning disable S3925 // Update implementation of "ISerializable"
    public class ServiceException : Exception, ISerializable
#pragma warning restore S3925 // Update implementation of "ISerializable"
    {
        public ServiceException(int errorcode, int statuscode, string reasonphrase, string message)
            : base(message)
        {
            this.ErrorCode = errorcode;
            this.StatusCode = statuscode;
            this.ReasonPhrase = reasonphrase;
        }

        public int ErrorCode { get; private set; }

        public int StatusCode { get; private set; }

        public string ReasonPhrase { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
