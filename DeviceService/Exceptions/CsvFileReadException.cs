namespace DeviceService.Exceptions
{
    public class CsvFileReadException : Exception
    {
        public CsvFileReadException() : base("Error occurred while reading the CSV file.") { }

        public CsvFileReadException(string message) : base(message) { }

        public CsvFileReadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
