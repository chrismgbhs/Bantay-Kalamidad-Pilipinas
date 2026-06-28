using System;
using System.Data;
using System.Linq;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    /// <summary>
    /// Backs rescuedashboard_Announcements_view.xaml, which has exactly two
    /// text fields — UrgentAnnouncement and UpdateAnnouncement — not a list.
    /// Reads from the new [Announcements] table (see
    /// Announcements_Schema.sql) via dbo.GetLatestAnnouncements(), which
    /// returns the most recent message of each type in a single row.
    /// </summary>
    internal class rescuedashboard_Announcements_ViewModel : ObservableObject
    {
        private string _UrgentAnnouncement;
        public string UrgentAnnouncement
        {
            get => _UrgentAnnouncement;
            set { _UrgentAnnouncement = value; OnPropertyChanged(nameof(UrgentAnnouncement)); }
        }

        private string _UpdateAnnouncement;
        public string UpdateAnnouncement
        {
            get => _UpdateAnnouncement;
            set { _UpdateAnnouncement = value; OnPropertyChanged(nameof(UpdateAnnouncement)); }
        }

        public rescuedashboard_Announcements_ViewModel()
        {
            InitializeAnnouncements();
        }

        public void InitializeAnnouncements()
        {
            string query = "SELECT * FROM dbo.GetLatestAnnouncements()";

            DatabaseManager.GetTableData(query, null, out DataTable data);

            if (data.Rows.Count > 0)
            {
                UrgentAnnouncement = data.Rows[0]["UrgentMessage"] == DBNull.Value
                    ? "No urgent announcements right now."
                    : data.Rows[0]["UrgentMessage"].ToString();

                UpdateAnnouncement = data.Rows[0]["UpdateMessage"] == DBNull.Value
                    ? "No updates right now."
                    : data.Rows[0]["UpdateMessage"].ToString();
            }
            else
            {
                UrgentAnnouncement = "No urgent announcements right now.";
                UpdateAnnouncement = "No updates right now.";
            }
        }
    }
}