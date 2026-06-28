namespace Bantay_Kalamidad_Pilipinas.Model
{
    /// <summary>
    /// One row in the "Current Rescue Operations" card's scrollable list —
    /// one row per operation this volunteer is part of, rather than a
    /// single aggregated breakdown string.
    /// </summary>
    internal class CurrentRescueOperationSummary
    {
        public string CurrentOperationId { get; set; }
        public string CurrentOperationStatus { get; set; }
        public string CurrentOperationLocation { get; set; }
        public string CurrentOperationDateStarted { get; set; }

        public CurrentRescueOperationSummary(string operationId, string status, string location, string dateStarted)
        {
            CurrentOperationId = operationId;
            CurrentOperationStatus = status;
            CurrentOperationLocation = location;
            CurrentOperationDateStarted = dateStarted;
        }
    }
}