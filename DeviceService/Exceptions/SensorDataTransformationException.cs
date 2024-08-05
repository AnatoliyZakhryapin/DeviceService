namespace DeviceService.Exceptions
{
    public class SensorDataTransformationException : Exception
    {
        public SensorDataTransformationException() : base("Error occurred during the transformation of sensor data.") { }

        public SensorDataTransformationException(string message) : base(message) { }

        public SensorDataTransformationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
