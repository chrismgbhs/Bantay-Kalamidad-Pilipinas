namespace Bantay_Kalamidad_Pilipinas.Model
{
    /// <summary>
    /// One row in the "My Assigned Operations" card's scrollable list.
    /// Property names match the per-item bindings in
    /// rescuedashboard_mainlayout_view.xaml's AssignedOperations
    /// ItemsControl DataTemplate.
    /// </summary>
    internal class AssignedOperation
    {
        public string AssignedOperationRole { get; set; }
        public string AssignedOperationLocation { get; set; }
        public string AssignedOperationId { get; set; }
        public string AssignedOperationStatus { get; set; }

        public AssignedOperation(string role, string location, string operationId, string status)
        {
            AssignedOperationRole = role;
            AssignedOperationLocation = location;
            AssignedOperationId = operationId;
            AssignedOperationStatus = status;
        }
    }
}