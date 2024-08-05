namespace DeviceService.Exceptions
{
    public class DataSendingException : Exception
    {
        public DataSendingException() : base("Error occurred while sending data to the server.") { }

        public DataSendingException(string message) : base(message) { }

        public DataSendingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
