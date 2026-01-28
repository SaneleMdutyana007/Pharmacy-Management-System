namespace PharmacyManager.Models.ViewModels
{
    public class DoctorVM
    {
        public int Id { get; set; } = 0;
        public string DoctorName { get; set; }
        public string DoctorSurname { get; set; }
        public string PracticeNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
    public class DoctorCreateVM
    {
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSurname { get; set; } = string.Empty;
        public string PracticeNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class DoctorEditVM
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSurname { get; set; } = string.Empty;
        public string PracticeNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}