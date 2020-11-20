using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Enums;
using PDR.PatientBooking.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly PatientBookingContext _context;

        public BookingController(PatientBookingContext context)
        {
            _context = context;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointnemtn(long identificationNumber)
        {
            var bookings = _context.Order.OrderBy(x => x.StartTime).ToList();
            var patientBookings = bookings.Where(p => p.PatientId == identificationNumber);

            if (!bookings.Any())
                return StatusCode(502,"Patient has no appointments");
            else
            {
                var hasFutureBookings = patientBookings.Where(p => p.StartTime > DateTime.Now);
                if (!hasFutureBookings.Any())
                    return StatusCode(502, "Patient has no furture bookings");

                var nextCancelledBooking = hasFutureBookings
                                            .Where(p => p.StartTime > DateTime.Now)
                                            .FirstOrDefault(p => p.OrderSatus == (int)OrderStatus.Cancelled);

                return Ok(new
                {
                    nextCancelledBooking.Id,
                    nextCancelledBooking.DoctorId,
                    nextCancelledBooking.StartTime,
                    nextCancelledBooking.EndTime,
                    Status = Enum.GetName(typeof(OrderStatus), nextCancelledBooking.OrderSatus)
                    }) ;
            }
        }

        [HttpPost("CancelAppointment")]
        public IActionResult CancelAppointment(Guid identificationNumber)
        {
            if (identificationNumber == default) 
                return StatusCode(400, "Order identification number is missing");

            var order = _context.Order.FirstOrDefault(p => p.Id == identificationNumber);
            if (order == null) return StatusCode(502, "Unknown order numbder");

            order.OrderSatus = (int)OrderStatus.Cancelled;
            _context.SaveChanges();

            return StatusCode(200, "Appointment has been cancelled");
        }

        [HttpPost()]
        public IActionResult AddBooking(NewBooking newBooking)
        {
            // Appt should not be in the past
            if (newBooking.StartTime < DateTime.Now) 
                return StatusCode(502, "Appointment requested in the past");

            var bookingId = new Guid();
            var bookingStartTime = newBooking.StartTime;
            var bookingEndTime = newBooking.EndTime;
            var bookingPatientId = newBooking.PatientId;
            var bookingPatient = GetPatientDetails(newBooking.PatientId);
            var bookingDoctorId = newBooking.DoctorId;
            var bookingDoctor = GetDoctorDetails(newBooking.DoctorId);
            var bookingSurgeryType = GetSurgeryType(bookingPatientId);

            var myBooking = new Order()
            {
                Id = bookingId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                PatientId = bookingPatientId,
                DoctorId = bookingDoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType,
                OrderSatus = (int)OrderStatus.Active
            };

            // This is used to check to see if the doctor has an 
            // appt at that time already
            if (IsDoctorBooked(newBooking)) 
                return StatusCode(502,"Doctor Unavailable");

            _context.Order.AddRange(new List<Order> { myBooking });
            _context.SaveChanges();

            return StatusCode(200);
        }

        private bool IsDoctorBooked(NewBooking newBooking)
        {
            return _context.Order
                .Where(p=> p.DoctorId == newBooking.DoctorId)
                .Any(p => p.StartTime == newBooking.StartTime && p.EndTime <= newBooking.StartTime && p.OrderSatus == (int)OrderStatus.Active);
        }

        private Patient GetPatientDetails(long PatientId) => _context.Patient.FirstOrDefault(x => x.Id == PatientId);

        private Doctor GetDoctorDetails(long DoctorId) => _context.Doctor.FirstOrDefault(x => x.Id == DoctorId);

        private SurgeryType GetSurgeryType(long PatientId)
        {
            // Assumption this would never be null
            var clinic = _context.Patient.FirstOrDefault(x => x.Id == PatientId).Clinic;
            return clinic.SurgeryType;
        }

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            MyOrderResult latestBooking;
            latestBooking = new MyOrderResult();
            latestBooking.Id = bookings2[i].Id;
            latestBooking.DoctorId = bookings2[i].DoctorId;
            latestBooking.StartTime = bookings2[i].StartTime;
            latestBooking.EndTime = bookings2[i].EndTime;
            latestBooking.PatientId = bookings2[i].PatientId;
            latestBooking.SurgeryType = (int)bookings2[i].GetSurgeryType();

            return latestBooking;
        }
    }
}