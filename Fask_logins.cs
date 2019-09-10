using System.ComponentModel.DataAnnotations;

namespace sklabelimportusers {
    public class Fask_logins {
        [Key] public string ID { get; set; }
        [Required] public string firstname { get; set; }
        [Required] public string surname { get; set; }
        public string psswd { get; set; }
        public string rfid { get; set; }
        public string barcode { get; set; }
    }
}