using System;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    /// <summary>
    /// Backs the "Active Disaster Event" ItemsControl in
    /// rescuedashboard_mainlayout_view.xaml. Property names match the XAML's
    /// per-item DataTemplate bindings exactly:
    ///   Run Text="{Binding ActiveDisasterEvent}"        -> EventName below
    ///   TextBlock Text="{Binding ActiveDisasterEventStatus}"
    ///   Run Text="{Binding ActiveDisasterEventId}"
    ///   Run Text="{Binding ActiveDisasterStartDate}"
    ///
    /// NOTE: the XAML names the first one "ActiveDisasterEvent", same as the
    /// ItemsControl's ItemsSource binding name ("DisasterEvent") and the
    /// existing Model.DisasterEvent class — three different things sharing
    /// very similar names. To avoid confusion this class is named
    /// RescueActiveDisasterEvent and its property is EventName; WPF binds by
    /// property name inside the DataTemplate, not by class name, so this
    /// works regardless of what the class itself is called.
    /// </summary>
    internal class RescueActiveDisasterEvent
    {
        public string ActiveDisasterEvent { get; set; }       // event name (XAML's "Event Name:" Run)
        public string ActiveDisasterEventStatus { get; set; } // e.g. "Ongoing"
        public string ActiveDisasterEventId { get; set; }
        public string ActiveDisasterStartDate { get; set; }

        public RescueActiveDisasterEvent(string eventName, string status, string eventId, string startDate)
        {
            ActiveDisasterEvent = eventName;
            ActiveDisasterEventStatus = status;
            ActiveDisasterEventId = eventId;
            ActiveDisasterStartDate = startDate;
        }
    }
}