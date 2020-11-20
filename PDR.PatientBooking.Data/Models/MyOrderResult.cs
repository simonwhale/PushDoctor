using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Data.Models
{
    public  class MyOrderResult
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public int SurgeryType { get; set; }
    }
}
