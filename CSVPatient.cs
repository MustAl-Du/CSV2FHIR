using System;
using System.Collections.Generic;
using System.Text;

namespace CSV2FHIR
{
    class CSVPatient
    {
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? SSN { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? MaterialStatus { get; set; }
        public string? Race { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public int? ZipCode { get; set; }
        public string? State{ get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EMail { get; set; }
        
    }
}
