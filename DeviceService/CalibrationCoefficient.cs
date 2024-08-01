namespace DeviceService
{
    // This coefficent just for example 
    public static class CalibrationCoefficient
    {
        private static double _offset = 10.0; 
        private static double _scaleFactor = 0.1; 

        public static double TransformValue(double rawValue)
        {
            return (rawValue - _offset) * _scaleFactor;
        }
    }
}
