using PDR.PatientBooking.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PDR.PatientBooking.Data.Models
{
    public class CancelAppointment
    {
        public long patientId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long DoctorId { get; set; }

        public SurgeryType SurgeryType { get; set; }
    }
}