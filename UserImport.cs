using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace linaplastimportusers {
    public class UserImport {
        [Key] public int OID { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string Name { get; set; }
        public string Rfid { get; set; }
    }
}