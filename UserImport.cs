using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace sklabelimportusers {
    public class UserImport {
        [Key] public string OID { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string Name { get; set; }
        public string Rfid { get; set; }
        public string Password { get; set; }
    }
}