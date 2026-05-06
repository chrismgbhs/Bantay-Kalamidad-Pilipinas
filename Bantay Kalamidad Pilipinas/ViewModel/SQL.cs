using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class SQL
    {
        //public static string connectionString = @"Data Source=CHREGION\SQLEXPRESS;Initial Catalog=""Bantay Kalamidad Pilipinas"";Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=""SQL Server Management Studio""";
        public static string connectionString = @"Data Source=ccl2-34;Initial Catalog=""Bantay Kalamidad Pilipinas"";Persist Security Info=True;User ID=sa;Password=ccl2;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=""SQL Server Management Studio"";";
    }
}
